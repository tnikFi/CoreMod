using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

/// <summary>
///     A guild-specific public role that users can assign to themselves
/// </summary>
public class PublicRole
{
    /// <summary>
    ///     ID of the guild where the role is available
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     ID of the role
    /// </summary>
    [Key]
    public ulong RoleId { get; set; }
}