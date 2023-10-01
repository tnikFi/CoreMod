using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Moderation.ExpireBan;

/// <summary>
///     Unban a user after a ban expires.
/// </summary>
public class ExpireBanQuery : IRequest
{
    /// <summary>
    ///     ID of the guild where the ban occurred.
    /// </summary>
    public required ulong GuildId { get; init; }

    /// <summary>
    ///     Moderation ID of the ban.
    /// </summary>
    public required int ModerationId { get; init; }
}

public class ExpireBanQueryHandler : IRequestHandler<ExpireBanQuery>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDiscordClient _discordClient;
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public ExpireBanQueryHandler(IDiscordClient discordClient, IMediator mediator,
        IModerationMessageService moderationMessageService, ApplicationDbContext dbContext)
    {
        _discordClient = discordClient;
        _mediator = mediator;
        _moderationMessageService = moderationMessageService;
        _dbContext = dbContext;
    }

    public async Task Handle(ExpireBanQuery request, CancellationToken cancellationToken)
    {
        var guild = await _discordClient.GetGuildAsync(request.GuildId);
        if (guild is null)
            throw new InvalidOperationException("Guild not found.");

        var guildBans = _dbContext.Moderations.AsNoTracking()
            .Where(x => x.GuildId == request.GuildId && x.Type == ModerationType.Ban)
            .ToArray();

        var moderation = guildBans.FirstOrDefault(x => x.Id == request.ModerationId);
        if (moderation is null)
            throw new InvalidOperationException("Moderation not found.");

        var ban = await guild.GetBanAsync(moderation.UserId);

        // If the user is not banned, do nothing
        if (ban is null) return;

        // Check that the user doesn't have new bans
        var newBan = guildBans.FirstOrDefault(x => x.UserId == moderation.UserId && x.Timestamp > moderation.Timestamp);

        // If the user has a new ban, don't unban the user
        if (newBan is not null) return;

        // If the ban has been pardoned or the expiration date is in the future, don't unban the user
        if (moderation.RelatedCase is not null || !(moderation.ExpiresAt <= DateTime.UtcNow)) return;

        await guild.RemoveBanAsync(moderation.UserId, new RequestOptions
        {
            AuditLogReason = $"Ban from case #{moderation.Id} expired."
        });

        // Message the user about the ban expiration and log the event to audit logs
        await _moderationMessageService.SendBanExpirationMessageAsync(moderation);
    }
}