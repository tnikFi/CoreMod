using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

public class WarnUserCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to warn the user in.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to warn.
    /// </summary>
    public required IGuildUser User { get; set; }

    /// <summary>
    ///     Moderator who issued the warning.
    /// </summary>
    public required IGuildUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the warning.
    /// </summary>
    public string? Reason { get; set; }
}

public class WarnUserCommandHandler : IRequestHandler<WarnUserCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public WarnUserCommandHandler(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Domain.Models.Moderation> Handle(WarnUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = await _mediator.Send(new AddModerationCommand
        {
            Guild = request.Guild,
            User = request.User,
            Moderator = request.Moderator,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Warning
        }, cancellationToken);

        return moderation;
    }
}