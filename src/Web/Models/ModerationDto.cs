using Domain.Models;

namespace Web.Models;

public class ModerationDto
{
    public required int Id { get; set; }
    public required string Type { get; set; }
    public string? Reason { get; set; }
    public required ulong UserId { get; set; }
    public ulong? ModeratorId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     Map a <see cref="Moderation" /> to a <see cref="ModerationDto" />.
    /// </summary>
    /// <param name="moderation">Moderation to map.</param>
    /// <param name="includeModerator">
    ///     Whether to include the moderator ID in the DTO. <see langword="false" /> by default.
    /// </param>
    /// <returns></returns>
    public static ModerationDto FromDomainModel(Moderation moderation, bool includeModerator = false)
    {
        return new ModerationDto
        {
            Id = moderation.Id,
            Type = moderation.Type.ToString(),
            Reason = moderation.Reason,
            UserId = moderation.UserId,
            ModeratorId = includeModerator ? moderation.ModeratorId : null,
            CreatedAt = new DateTimeOffset(moderation.Timestamp, TimeSpan.Zero),
            ExpiresAt = moderation.ExpiresAt.HasValue
                ? new DateTimeOffset(moderation.ExpiresAt.Value, TimeSpan.Zero)
                : null
        };
    }
}