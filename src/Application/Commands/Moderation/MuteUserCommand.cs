using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

public class MuteUserCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to mute the user in.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to mute.
    /// </summary>
    public required IGuildUser User { get; set; }

    /// <summary>
    ///     Moderator muting the user.
    /// </summary>
    public required IGuildUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the mute.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    ///     Duration of the mute. If not provided, the mute will be permanent.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.MaxValue;
}

public class MuteUserCommandHandler : IRequestHandler<MuteUserCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;

    public MuteUserCommandHandler(ApplicationDbContext dbContext,
        IModerationMessageService moderationMessageService)
    {
        _dbContext = dbContext;
        _moderationMessageService = moderationMessageService;
    }

    public async Task<Domain.Models.Moderation> Handle(MuteUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Mute
        };

        await request.User.SetTimeOutAsync(request.Duration, new RequestOptions
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