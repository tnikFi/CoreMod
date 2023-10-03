using Application.Interfaces;
using Application.Queries.Moderation;
using Discord;
using Domain.Enums;
using Domain.Models;
using Hangfire;
using MediatR;

namespace Application.Services;

public class UnbanSchedulingService : IUnbanSchedulingService
{
    // Use Hangfire to schedule the unban
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IDiscordClient _discordClient;
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public UnbanSchedulingService(IBackgroundJobClient backgroundJobClient, IDiscordClient discordClient,
        IMediator mediator, IModerationMessageService moderationMessageService)
    {
        _backgroundJobClient = backgroundJobClient;
        _discordClient = discordClient;
        _mediator = mediator;
        _moderationMessageService = moderationMessageService;
    }

    /// <inheritdoc />
    public void ScheduleBanExpiration(Moderation moderation)
    {
        if (moderation.Type != ModerationType.Ban)
            throw new InvalidOperationException("The moderation type must be a ban.");
        if (!moderation.ExpiresAt.HasValue)
            throw new InvalidOperationException("The moderation must have an expiration date.");
        if (!moderation.Active)
            throw new InvalidOperationException("The moderation must be active.");

        _backgroundJobClient.Schedule(() => ExpireBan(moderation.GuildId, moderation.Id), moderation.ExpiresAt.Value);
    }

    /// <summary>
    ///     Unban a user from a guild.
    /// </summary>
    /// <param name="guildId">ID of the guild the moderation is from</param>
    /// <param name="moderationId">ID of the moderation that banned the user</param>
    // ReSharper disable once MemberCanBePrivate.Global - Used by Hangfire
    public async Task ExpireBan(ulong guildId, int moderationId)
    {
        await _mediator.Send(new ExpireBanQuery
        {
            GuildId = guildId,
            ModerationId = moderationId
        });
    }
}