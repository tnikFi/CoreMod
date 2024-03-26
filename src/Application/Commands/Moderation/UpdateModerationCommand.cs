using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

public class UpdateModerationCommand : IRequest
{
    /// <summary>
    ///     Updated moderation entity. Should have the same ID as the original entity. Should be originally retrieved
    ///     from the database with tracking enabled, as some validation is done based on the original values.
    /// </summary>
    public required Domain.Models.Moderation Moderation { get; set; }

    /// <summary>
    ///     Forcibly allow changing the JobId of the moderation entity. This should only be used when the JobId is set
    ///     somewhere else and this command is used to update the moderation entity to reflect that change. Note that
    ///     this will not update the scheduled job itself, only the entity in the database.
    /// </summary>
    /// <remarks>
    ///     Setting this to <see langword="true" /> while also updating <see cref="Domain.Models.Moderation.ExpiresAt" />
    ///     may cause unexpected behavior.
    /// </remarks>
    public bool? ForceAllowJobIdChange { get; init; }
}

public class UpdateModerationCommandHandler : IRequestHandler<UpdateModerationCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IDiscordClient _discordClient;
    private readonly IUnbanSchedulingService _unbanSchedulingService;

    public UpdateModerationCommandHandler(ApplicationDbContext context, IUnbanSchedulingService unbanSchedulingService,
        IDiscordClient discordClient)
    {
        _context = context;
        _unbanSchedulingService = unbanSchedulingService;
        _discordClient = discordClient;
    }

    public async Task Handle(UpdateModerationCommand request,
        CancellationToken cancellationToken)
    {
        // Attempt to find the entity in the database.
        var entity = await _context.Moderations.FindAsync(new object?[] {request.Moderation.Id}, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException("The moderation does not exist.");

        // Get the entry for change tracking.
        var entityEntry = _context.Entry(request.Moderation);

        // Throw an exception if the moderation type has been changed.
        if (entityEntry.Property(x => x.Type).IsModified)
            throw new InvalidOperationException("The moderation type cannot be changed.");
        
        // Throw an exception if the guild ID has been changed.
        if (entityEntry.Property(x => x.GuildId).IsModified)
            throw new InvalidOperationException("The guild ID cannot be changed.");

        // Don't allow changing the scheduled JobId directly.
        if (entityEntry.Property(x => x.JobId).IsModified && request.ForceAllowJobIdChange is not true)
            throw new InvalidOperationException("The scheduled job ID cannot be changed directly.");

        // Don't allow changing the expiration time to a time in the past.
        var expiresAt = entityEntry.Property(x => x.ExpiresAt);
        if (expiresAt.IsModified && request.Moderation.ExpiresAt < DateTimeOffset.UtcNow)
            throw new InvalidOperationException("The expiration time cannot be in the past.");

        switch (request.Moderation.Type)
        {
            // Temporary bans are handled by the bot, so if a ban has been rescheduled, the associated background job must
            // be updated.
            case ModerationType.Ban when expiresAt.IsModified:
            {
                // If the ban is now permanent, cancel the scheduled unban job.
                if (request.Moderation.ExpiresAt is null)
                {
                    var removed = _unbanSchedulingService.CancelBanExpiration(request.Moderation);
                    if (removed)
                        request.Moderation.JobId = null;
                }
                // If the ban didn't have a scheduled unban job, schedule one.
                else if (expiresAt.OriginalValue is null)
                {
                    request.Moderation.JobId = _unbanSchedulingService.ScheduleBanExpiration(request.Moderation);
                }
                // Otherwise, a scheduled unban job exists so the ban expiration must be rescheduled.
                else
                {
                    _unbanSchedulingService.UpdateBanExpiration(request.Moderation);
                }

                break;
            }
            // Update the mute duration if it has been modified.
            case ModerationType.Mute when expiresAt.IsModified:
            {
                var guild = await _discordClient.GetGuildAsync(request.Moderation.GuildId);
                if (guild is null) break;
                var user = await guild.GetUserAsync(request.Moderation.UserId);
                if (user is null) break;
                var duration = request.Moderation.ExpiresAt is not null
                    ? (TimeSpan) (request.Moderation.ExpiresAt - DateTimeOffset.UtcNow)
                    : TimeSpan.MaxValue;
                await user.SetTimeOutAsync(duration, new RequestOptions
                {
                    AuditLogReason = "Mute duration updated."
                });
                break;
            }
        }

        // Update the entity with the new values.
        _context.Entry(entity).CurrentValues.SetValues(request.Moderation);

        await _context.SaveChangesAsync(cancellationToken);
    }
}