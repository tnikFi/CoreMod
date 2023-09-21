using Discord.WebSocket;

namespace Infrastructure.Configuration;

/// <summary>
/// Configs for the Discord client.
/// </summary>
public class DiscordConfiguration
{
    public string? BotToken { get; set; }
    public string DefaultPrefix { get; set; } = "!";
    public DiscordSocketConfig SocketConfig { get; set; } = new();
}