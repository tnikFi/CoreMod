using Application.Interfaces;
using Discord;
using Domain.Enums;
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
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public UnmuteUserCommandHandler(IModerationMessageService moderationMessageService, IMediator mediator)
    {
        _moderationMessageService = moderationMessageService;
        _mediator = mediator;
    }

    public async Task<Domain.Models.Moderation> Handle(UnmuteUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = await _mediator.Send(new AddModerationCommand
        {
            Guild = request.Guild,
            User = request.User,
            Moderator = request.Moderator,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Unmute
        }, cancellationToken);

        await request.User.RemoveTimeOutAsync(new RequestOptions
        {
            AuditLogReason = request.Reason
        });

        // Log the moderation
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}