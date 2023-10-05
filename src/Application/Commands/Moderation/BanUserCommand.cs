using Application.Interfaces;
using Discord;
using Domain.Enums;
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
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;
    private readonly IUnbanSchedulingService _unbanSchedulingService;

    public BanUserCommandHandler(IModerationMessageService moderationMessageService,
        IUnbanSchedulingService unbanSchedulingService, IMediator mediator)
    {
        _moderationMessageService = moderationMessageService;
        _unbanSchedulingService = unbanSchedulingService;
        _mediator = mediator;
    }

    public async Task<Domain.Models.Moderation> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = await _mediator.Send(new AddModerationCommand
        {
            Guild = request.Guild,
            User = request.User,
            Moderator = request.Moderator,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Ban,
            Duration = request.Duration
        }, cancellationToken);

        // Message the user before banning them to make sure the message can be delivered
        await _moderationMessageService.SendModerationMessageAsync(moderation, false);
        await request.Guild.AddBanAsync(request.User, request.PruneDays, request.Reason);

        // Schedule the unban if the moderation is a temporary ban
        if (moderation is {ExpiresAt: not null})
            _unbanSchedulingService.ScheduleBanExpiration(moderation);

        // Send the moderation message if requested
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}