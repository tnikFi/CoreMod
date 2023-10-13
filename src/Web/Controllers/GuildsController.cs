using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GuildsController : ControllerBase
{
    private readonly DiscordSocketClient _discordClient;

    public GuildsController(DiscordSocketClient discordClient)
    {
        _discordClient = discordClient;
    }

    [HttpGet("{guildId}/member-count")]
    public async Task<ActionResult<int>> GetGuildMemberCountAsync(string guildId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return BadRequest();
        var guild = _discordClient.GetGuild(guildIdParsed);

        if (guild is null) return NotFound();

        return Ok(guild.MemberCount);
    }
}