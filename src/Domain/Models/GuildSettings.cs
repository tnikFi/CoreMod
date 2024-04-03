using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class GuildSettings
{
    /// <summary>
    ///     Id of the guild
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Override the default command prefix for this guild. Null if default.
    /// </summary>
    public string? CommandPrefix { get; set; }

    /// <summary>
    ///     Welcome message to send to new members. Null if disabled.
    /// </summary>
    public string? WelcomeMessage { get; set; }

    /// <summary>
    ///     Id of the channel to send moderation logs to. Null if disabled.
    /// </summary>
    public ulong? LogChannelId { get; set; }

    /// <summary>
    ///     Id of the channel to send user reports to. Null if disabled.
    /// </summary>
    public ulong? ReportChannelId { get; set; }

    /// <summary>
    ///     Lowest role that can use the report command. Null if everyone can use it.
    /// </summary>
    public ulong? MinimumReportRole { get; set; }
}