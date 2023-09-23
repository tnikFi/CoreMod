using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models;

[PrimaryKey(nameof(GuildId), nameof(CaseNumber))]
public class Moderation
{
    public ulong GuildId { get; set; }
    
    public ulong UserId { get; set; }
    
    public ulong ModeratorId { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    [MaxLength(2000)]
    public string? Reason { get; set; }
    
    // Make case numbers auto increment for each guild
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CaseNumber { get; set; }
    
    /// <summary>
    /// If the moderation action is used to pardon another case, this will be the case number of the pardoned action.
    /// </summary>
    public int? PardonedCaseNumber { get; set; }
    
    [Required]
    public ModerationType Type { get; set; }
}