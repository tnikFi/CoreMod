using Application.Interfaces;
using Common.Utils;
using Discord;
using Domain.Attributes;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation.ModerateUser;

public class ModerateUserCommand : IRequest<Domain.Models.Moderation>
{
    public IGuild Guild { get; set; }
    public IGuildUser User { get; set; }
    public IGuildUser Moderator { get; set; }
    public string? Reason { get; set; }
    public ModerationType Type { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool SendModerationMessage { get; set; } = true;
}

public class ModerateUserCommandHandler : IRequestHandler<ModerateUserCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;

    public ModerateUserCommandHandler(ApplicationDbContext dbContext,
        IModerationMessageService moderationMessageService)
    {
        _dbContext = dbContext;
        _moderationMessageService = moderationMessageService;
    }

    public async Task<Domain.Models.Moderation> Handle(ModerateUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = request.Type
        };

        // Apply the moderation to the user. Warning isn't a Discord action, so it doesn't need to be applied.
        switch (request.Type)
        {
            case ModerationType.Mute:
                await request.User.SetTimeOutAsync(request.Duration ?? TimeSpan.MaxValue, new RequestOptions
                {
                    AuditLogReason = request.Reason
                });
                break;
            case ModerationType.Kick:
                await request.User.KickAsync(request.Reason);
                break;
            case ModerationType.Ban:
                await request.Guild.AddBanAsync(request.User, 0, request.Reason);
                break;
            case ModerationType.Unmute:
                await request.User.RemoveTimeOutAsync(new RequestOptions
                {
                    AuditLogReason = request.Reason
                });
                break;
            case ModerationType.Unban:
                await request.Guild.RemoveBanAsync(request.User, new RequestOptions
                {
                    AuditLogReason = request.Reason
                });
                break;
        }
        
        if (EnumUtils.HasAttribute<CanBeTemporaryAttribute>(moderation.Type) && request.Duration.HasValue)
            moderation.ExpiresAt = DateTime.UtcNow + request.Duration.Value;

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send the moderation message if requested
        if (request.SendModerationMessage)
            await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}