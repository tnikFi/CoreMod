using Application.Interfaces;
using Discord;
using Domain.Enums;
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
    public TimeSpan? Duration { get; set; }
}

public class MuteUserCommandHandler : IRequestHandler<MuteUserCommand, Domain.Models.Moderation>
{
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public MuteUserCommandHandler(IModerationMessageService moderationMessageService, IMediator mediator)
    {
        _moderationMessageService = moderationMessageService;
        _mediator = mediator;
    }

    public async Task<Domain.Models.Moderation> Handle(MuteUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = await _mediator.Send(new AddModerationCommand
        {
            Guild = request.Guild,
            User = request.User,
            Moderator = request.Moderator,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Mute,
            Duration = request.Duration
        }, cancellationToken);

        await request.User.SetTimeOutAsync(request.Duration ?? TimeSpan.MaxValue, new RequestOptions
        {
            AuditLogReason = request.Reason
        });

        // Log the moderation
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}