using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

public class BanUserCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to ban the user from.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to ban.
    /// </summary>
    public required IUser User { get; set; }

    /// <summary>
    ///     Moderator banning the user.
    /// </summary>
    public required IGuildUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the ban.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    ///     Duration of the ban.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    ///     How many days of messages from the banned user should be pruned.
    /// </summary>
    public int PruneDays { get; set; } = 0;
}

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;
    private readonly IUnbanSchedulingService _unbanSchedulingService;

    public BanUserCommandHandler(ApplicationDbContext dbContext, IModerationMessageService moderationMessageService,
        IUnbanSchedulingService unbanSchedulingService)
    {
        _dbContext = dbContext;
        _moderationMessageService = moderationMessageService;
        _unbanSchedulingService = unbanSchedulingService;
    }

    public async Task<Domain.Models.Moderation> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Ban
        };

        // Set the expiration date if a duration was provided
        if (request.Duration.HasValue)
            moderation.ExpiresAt = DateTime.UtcNow + request.Duration.Value;

        // Message the user before banning them to make sure the message can be delivered
        await _moderationMessageService.SendModerationMessageAsync(moderation, false);
        await request.Guild.AddBanAsync(request.User, request.PruneDays, request.Reason);

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Schedule the unban if the moderation is a temporary ban
        if (moderation is {ExpiresAt: not null})
            _unbanSchedulingService.ScheduleBanExpiration(moderation);

        // Send the moderation message if requested
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}