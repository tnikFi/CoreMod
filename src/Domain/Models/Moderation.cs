using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Utils;
using Domain.Attributes;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models;

[PrimaryKey(nameof(Id))]
public class Moderation
{
    [NotMapped] private DateTime? _expiresAt;

    /// <summary>
    ///     Id of the moderation action
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    ///     ID of the guild where the moderation action was performed
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     ID of the user who was moderated
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    ///     ID of the moderator who performed the action
    /// </summary>
    public ulong ModeratorId { get; set; }

    /// <summary>
    ///     Timestamp of the moderation action
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Expiration time of the moderation action. Can only be set if the moderation action type allows temporary
    ///     moderation actions.
    /// </summary>
    public DateTime? ExpiresAt
    {
        get => _expiresAt;
        // Only allow setting the expiration date if the moderation action type can be temporary and the is active
        set => _expiresAt = EnumUtils.HasAttribute<CanBeTemporaryAttribute>(Type) && Active ? value : _expiresAt;
    }

    /// <summary>
    ///     Reason for the moderation action given by the moderator
    /// </summary>
    [MaxLength(2000)]
    public string? Reason { get; set; }

    /// <summary>
    ///     Indicates whether or not the moderation action should be considered active.
    /// </summary>
    [NotMapped]
    public bool Active => (IsPardon || !IsPardoned) && !IsExpired;

    /// <summary>
    ///     If the moderation action revoked a previous action or was revoked itself, this will be the related case.
    /// </summary>
    public Moderation? RelatedCase { get; set; }

    /// <summary>
    ///     Moderation action that revoked this action, if any. Equal to <see cref="RelatedCase" /> if this
    ///     action has been pardoned.
    /// </summary>
    [NotMapped]
    public Moderation? Pardon => RelatedCase?.IsPardon is true ? RelatedCase : null;

    /// <summary>
    ///     Moderation action that was revoked by this action, if any. Equal to <see cref="RelatedCase" /> if this
    ///     action is a pardon.
    /// </summary>
    [NotMapped]
    public Moderation? PardonedCase => IsPardon ? RelatedCase : null;

    /// <summary>
    ///     True if the moderation action type is a pardon.
    /// </summary>
    [NotMapped]
    public bool IsPardon => EnumUtils.HasAttribute<PardonAttribute>(Type);

    /// <summary>
    ///     True if the moderation action has been pardoned.
    /// </summary>
    [NotMapped]
    public bool IsPardoned => Pardon is not null;

    /// <summary>
    ///     If the moderation action has an expiration date, this will be true if the expiration date has passed.
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    /// <summary>
    ///     Whether or not the moderation action is temporary.
    /// </summary>
    [NotMapped]
    public bool IsTemporary => ExpiresAt.HasValue;

    /// <summary>
    ///     Type of moderation action
    /// </summary>
    [Required]
    public ModerationType Type { get; set; }

    /// <summary>
    ///     Job ID of the scheduled task to unban the user after the ban expires.
    ///     Should be null if the moderation action is not a ban or if the ban is permanent.
    /// </summary>
    public string? JobId { get; set; }
}