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
    public string ScheduleBanExpiration(Moderation moderation)
    {
        if (moderation.Type != ModerationType.Ban)
            throw new InvalidOperationException("The moderation type must be a ban.");
        if (!moderation.ExpiresAt.HasValue)
            throw new InvalidOperationException("The moderation must have an expiration date.");
        if (!moderation.Active)
            throw new InvalidOperationException("The moderation must be active.");

        return _backgroundJobClient.Schedule(() => ExpireBan(moderation.GuildId, moderation.Id),
            moderation.ExpiresAt.Value);
    }

    /// <inheritdoc />
    public bool UpdateBanExpiration(Moderation moderation)
    {
        if (moderation.Type != ModerationType.Ban)
            throw new InvalidOperationException("The moderation type must be a ban.");
        if (!moderation.ExpiresAt.HasValue)
            throw new InvalidOperationException("The moderation must have an expiration date.");
        if (!moderation.Active)
            throw new InvalidOperationException("The moderation must be active.");
        if (string.IsNullOrEmpty(moderation.JobId))
            throw new InvalidOperationException("The moderation must have an associated scheduled unban job.");
        if (moderation.ExpiresAt.Value < DateTimeOffset.UtcNow)
            throw new InvalidOperationException("The new expiration time must be in the future.");

        return _backgroundJobClient.Reschedule(moderation.JobId, moderation.ExpiresAt.Value);
    }

    /// <inheritdoc />
    public bool CancelBanExpiration(Moderation moderation)
    {
        if (moderation.Type != ModerationType.Ban)
            throw new InvalidOperationException("The moderation type must be a ban.");
        if (!moderation.Active)
            throw new InvalidOperationException("The moderation must be active.");
        if (string.IsNullOrEmpty(moderation.JobId))
            throw new InvalidOperationException("The moderation must have an associated scheduled unban job.");

        return _backgroundJobClient.Delete(moderation.JobId);
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