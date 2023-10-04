using Application.Interfaces;
using Common.Utils;
using Discord;
using Domain.Attributes;
using Domain.Enums;
using Infrastructure.Data.Contexts;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;

    public KickUserCommandHandler(ApplicationDbContext dbContext,
        IModerationMessageService moderationMessageService)
    {
        _dbContext = dbContext;
        _moderationMessageService = moderationMessageService;
    }

    public async Task<Domain.Models.Moderation> Handle(KickUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = ModerationType.Kick
        };

        // Send the moderation message before kicking the user to make sure it can be delivered
        await _moderationMessageService.SendModerationMessageAsync(moderation, false);
        await request.User.KickAsync(request.Reason);

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send the moderation message
        await _moderationMessageService.SendModerationMessageAsync(moderation);

        return moderation;
    }
}