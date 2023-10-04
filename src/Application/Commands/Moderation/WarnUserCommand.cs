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
    public WarnUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Models.Moderation> Handle(WarnUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Warning
        };

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return moderation;
    }
}