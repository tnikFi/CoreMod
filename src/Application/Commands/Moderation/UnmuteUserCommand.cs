using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

public class UnmuteUserCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to unmute the user in.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to unmute.
    /// </summary>
    public required IGuildUser User { get; set; }

    /// <summary>
    ///     Moderator removing the mute.
    /// </summary>
    public required IGuildUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the unmute.
    /// </summary>
    public string? Reason { get; set; }
}

public class UnmuteUserCommandHandler : IRequestHandler<UnmuteUserCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;

    public UnmuteUserCommandHandler(ApplicationDbContext dbContext,
        IModerationMessageService moderationMessageService)
    {
        _dbContext = dbContext;
        _moderationMessageService = moderationMessageService;
    }

    public async Task<Domain.Models.Moderation> Handle(UnmuteUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Unmute
        };

        await request.User.RemoveTimeOutAsync(new RequestOptions
        {
            AuditLogReason = request.Reason
        });

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Log the moderation
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}