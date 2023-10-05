using Application.Interfaces;
using Discord;
using Domain.Enums;
using MediatR;

namespace Application.Commands.Moderation;

public class KickUserCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to kick the user from.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to kick.
    /// </summary>
    public required IGuildUser User { get; set; }

    /// <summary>
    ///     Moderator kicking the user.
    /// </summary>
    public required IGuildUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the kick.
    /// </summary>
    public string? Reason { get; set; }
}

public class KickUserCommandHandler : IRequestHandler<KickUserCommand, Domain.Models.Moderation>
{
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public KickUserCommandHandler(IModerationMessageService moderationMessageService, IMediator mediator)
    {
        _moderationMessageService = moderationMessageService;
        _mediator = mediator;
    }

    public async Task<Domain.Models.Moderation> Handle(KickUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = await _mediator.Send(new AddModerationCommand
        {
            Guild = request.Guild,
            User = request.User,
            Moderator = request.Moderator,
            Reason = request.Reason,
            Type = ModerationType.Kick
        }, cancellationToken);

        // Send the moderation message before kicking the user to make sure it can be delivered
        await _moderationMessageService.SendModerationMessageAsync(moderation, false);
        await request.User.KickAsync(request.Reason);

        // Send the moderation message
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}