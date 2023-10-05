using Application.Interfaces;
using Discord;
using Domain.Enums;
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
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public UnbanUserCommandHandler(IModerationMessageService moderationMessageService, IMediator mediator)
    {
        _moderationMessageService = moderationMessageService;
        _mediator = mediator;
    }

    public async Task<Domain.Models.Moderation> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = await _mediator.Send(new AddModerationCommand
        {
            Guild = request.Guild,
            User = request.User,
            Moderator = request.Moderator,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Unban
        }, cancellationToken);

        // Remove the ban
        await request.Guild.RemoveBanAsync(request.User, new RequestOptions
        {
            AuditLogReason = request.Reason
        });

        // Send the moderation message
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}