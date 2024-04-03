using Application.Interfaces;
using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Moderation;

public class DeleteModerationCommand : IRequest
{
    /// <summary>
    ///     Guild id to delete the case from
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Case id to delete
    /// </summary>
    public int CaseId { get; set; }

    /// <summary>
    ///     User who deleted the moderation
    /// </summary>
    public IGuildUser? DeletedBy { get; set; }

    /// <summary>
    ///     If true, the user will be unbanned or unmuted if the moderation is a ban or mute.
    /// </summary>
    public bool PardonModeration { get; set; }
}

public class DeleteModerationCommandHandler : IRequestHandler<DeleteModerationCommand>
{
    private readonly IDiscordClient _client;
    private readonly ApplicationDbContext _dbContext;
    private readonly IModerationMessageService _moderationMessageService;
    private readonly IUnbanSchedulingService _unbanSchedulingService;

    public DeleteModerationCommandHandler(IDiscordClient client, ApplicationDbContext dbContext,
        IUnbanSchedulingService unbanSchedulingService, IModerationMessageService moderationMessageService)
    {
        _client = client;
        _dbContext = dbContext;
        _unbanSchedulingService = unbanSchedulingService;
        _moderationMessageService = moderationMessageService;
    }

    public async Task Handle(DeleteModerationCommand request, CancellationToken cancellationToken)
    {
        var moderationCase = await _dbContext.Moderations
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId && x.Id == request.CaseId, cancellationToken);
        if (moderationCase is null) return;

        var guild = await _client.GetGuildAsync(moderationCase.GuildId);
        var guildUser = await guild.GetUserAsync(moderationCase.UserId);
        var userBan = await guild.GetBanAsync(moderationCase.UserId);

        var requestOptions = new RequestOptions
        {
            AuditLogReason = request.DeletedBy != null
                ? $"Moderation case #{moderationCase.Id} deleted by {request.DeletedBy.Mention}"
                : $"Moderation case #{moderationCase.Id} deleted"
        };

        switch (moderationCase.Type)
        {
            case ModerationType.Mute:
                var unmute = guildUser is {TimedOutUntil: not null} && request.PardonModeration;
                if (unmute)
                    await guildUser.RemoveTimeOutAsync(requestOptions);
                if (request.DeletedBy is not null)
                    await _moderationMessageService.SendModerationDeletedMessageAsync(moderationCase,
                        request.DeletedBy, unmute);
                break;
            case ModerationType.Ban:
                var unban = userBan is not null && request.PardonModeration;
                if (unban)
                {
                    await guild.RemoveBanAsync(moderationCase.UserId, requestOptions);
                    if (moderationCase is {Active: true, ExpiresAt: not null})
                        _unbanSchedulingService.CancelBanExpiration(moderationCase);
                }

                if (request.DeletedBy is not null)
                    await _moderationMessageService.SendModerationDeletedMessageAsync(moderationCase,
                        request.DeletedBy, unban);
                break;
            default:
                await _moderationMessageService.SendModerationDeletedMessageAsync(moderationCase, request.DeletedBy,
                    false);
                break;
        }

        _dbContext.Moderations.Remove(moderationCase);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}