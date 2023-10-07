using Application.Queries.Moderation;
using Discord.WebSocket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly DiscordSocketClient _discordClient;
    private readonly IMediator _mediator;

    public UserController(DiscordSocketClient discordClient, IMediator mediator)
    {
        _discordClient = discordClient;
        _mediator = mediator;
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

    /// <summary>
    ///     Get the moderation history of the user.
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    [HttpGet("moderations")]
    public async Task<ActionResult<ModerationDto[]>> GetModerations(ulong guildId)
    {
        var userId = ulong.Parse(User.Claims.First(c => c.Type == "userId").Value);
        var user = await _discordClient.Rest.GetUserAsync(userId);
        var guild = _discordClient.GetGuild(guildId);

        if (guild is null) return NotFound();

        var moderations = await _mediator.Send(new GetModerationsQuery
        {
            Guild = guild,
            User = user
        });

        return Ok(moderations.Select(m => new ModerationDto
        {
            Id = m.Id,
            Type = m.Type.ToString(),
            CreatedAt = m.Timestamp,
            UserId = m.UserId,
            Reason = m.Reason,
            ExpiresAt = m.ExpiresAt
        }));
    }
}