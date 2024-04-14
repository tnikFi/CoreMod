using Discord.WebSocket;

namespace Infrastructure.Configuration;

/// <summary>
/// Configs for the Discord client.
/// </summary>
public class DiscordConfiguration
{
    public string? BotToken { get; set; }
    public string DefaultPrefix { get; set; } = "!";
    public int MaxPrefixLength { get; set; } = 5;
    public int MaxPublicRoles { get; set; } = 25;
    public DiscordSocketConfig SocketConfig { get; set; } = new();
    public ulong? DebugGuildId { get; set; }
}