using Common.Utils;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

/// <summary>
///     Base controller for all controllers in the application. Provides useful methods and properties to reduce code
///     duplication.
/// </summary>
public class BaseController : ControllerBase
{
    protected readonly DiscordSocketClient DiscordClient;
    
    public BaseController(DiscordSocketClient discordClient)
    {
        DiscordClient = discordClient;
    }
    
    /// <summary>
    /// The Discord user ID of the current authenticated user.
    /// </summary>
    public ulong? DiscordUserId => ParseUtils.ParseUlong(User.Claims.First(c => c.Type == "userId").Value);
    
    /// <summary>
    /// The Discord user object of the current authenticated user.
    /// </summary>
    public SocketUser? DiscordUser => DiscordUserId.HasValue ? DiscordClient.GetUser(DiscordUserId.Value) : null;
    
    /// <summary>
    /// Get the guild user object of the current authenticated user in the specified guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns><see cref="SocketGuildUser"/> if the guild and user exist, otherwise null.</returns>
    protected SocketGuildUser? GetCurrentGuildUser(ulong guildId)
    {
        var guild = DiscordClient.GetGuild(guildId);
        return DiscordUserId != null ? guild?.GetUser(DiscordUserId.Value) : null;
    }
}