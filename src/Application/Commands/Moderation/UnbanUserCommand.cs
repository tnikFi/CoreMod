using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

public class UnbanUserCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to unban the user in.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to unban.
    /// </summary>
    public required IUser User { get; set; }

    /// <summary>
    ///     Moderator removing the ban.
    /// </summary>
    public required IGuildUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the unban.
    /// </summary>
    public string? Reason { get; set; }
}

public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;

    public UnbanUserCommandHandler(ApplicationDbContext dbContext,
        IModerationMessageService moderationMessageService)
    {
        _dbContext = dbContext;
        _moderationMessageService = moderationMessageService;
    }

    public async Task<Domain.Models.Moderation> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Unban
        };

        // Remove the ban
        await request.Guild.RemoveBanAsync(request.User, new RequestOptions
        {
            AuditLogReason = request.Reason
        });

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send the moderation message
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}