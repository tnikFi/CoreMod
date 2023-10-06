using Discord.WebSocket;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AuthConfiguration _authConfiguration;
    private readonly DiscordSocketClient _discordClient;
    private readonly HttpClient _httpClient;

    public UserController(AuthConfiguration authConfiguration, DiscordSocketClient discordClient)
    {
        _authConfiguration = authConfiguration;
        _discordClient = discordClient;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    [HttpGet("guilds")]
    public async Task<ActionResult<GuildDto[]>> GetGuilds()
    {
        // Get the user ID from the claims.
        var userId = ulong.Parse(User.Claims.First(c => c.Type == "userId").Value);

        // Download the users of all guilds the bot is in in parallel.
        var mutualGuilds = _discordClient.Guilds.Where(g => g.GetUser(userId) != null);

        var guildDtos = mutualGuilds.Select(g => new GuildDto
        {
            Id = g.Id,
            Name = g.Name,
            Icon = g.IconUrl
        }).ToArray();

        return Ok(guildDtos);
    }
}