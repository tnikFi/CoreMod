using Application.Queries.Configuration;
using Application.Queries.DiscordApiCalls;
using Application.Queries.Moderation;
using Discord;
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
            Id = g.Id.ToString(),
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
    public async Task<ActionResult<ModerationDto[]>> GetModerations(string guildId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return BadRequest();
        var userId = ulong.Parse(User.Claims.First(c => c.Type == "userId").Value);
        var user = await _discordClient.Rest.GetUserAsync(userId);
        var guild = _discordClient.GetGuild(guildIdParsed);

        if (guild is null) return NotFound();

        var moderations = await _mediator.Send(new GetModerationsQuery
        {
            Guild = guild,
            User = user
        });

        return moderations.Select(x => ModerationDto.FromDomainModel(x, false)).ToArray();
    }

    /// <summary>
    ///     Get the roles of the user in a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    [HttpGet("roles")]
    public async Task<ActionResult<RoleDto[]>> GetRolesAsync(string guildId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return BadRequest();
        var userId = ulong.Parse(User.Claims.First(c => c.Type == "userId").Value);
        var guildUser = await _discordClient.Rest.GetGuildUserAsync(guildIdParsed, userId);
        var guild = _discordClient.GetGuild(guildIdParsed);
        var roles = guild.Roles.Where(r => guildUser.RoleIds.Contains(r.Id))
            .Where(r => !r.IsEveryone)
            .Select(RoleDto.FromSocketRole)
            .ToArray();

        return Ok(roles);
    }

    /// <summary>
    ///     Get the permissions of the user in a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    [HttpGet("permissions")]
    public async Task<ActionResult<GuildPermissions>> GetGuildPermissionsAsync(string guildId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return BadRequest();
        var userId = ulong.Parse(User.Claims.First(c => c.Type == "userId").Value);
        var guildUser = await _discordClient.Rest.GetGuildUserAsync(guildIdParsed, userId);
        return Ok(guildUser.GuildPermissions);
    }

    /// <summary>
    ///     Update the public roles of the current user in a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    [HttpPatch("public-roles")]
    public async Task<ActionResult<RoleDto[]>> UpdatePublicRolesAsync(string guildId, [FromBody] RoleDto[] roles)
    {
        // If there are more than 25 roles, return bad request.
        if (roles.Length > 25) return BadRequest("Cannot assign more than 25 public roles.");

        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return BadRequest();
        var userId = ulong.Parse(User.Claims.First(c => c.Type == "userId").Value);
        var guildUser = await _discordClient.Rest.GetGuildUserAsync(guildIdParsed, userId);
        var guild = _discordClient.GetGuild(guildIdParsed);

        if (guild is null) return NotFound();

        // Parse the requested roles into a ulong list.
        var roleIds = new List<ulong>();
        foreach (var role in roles)
        {
            var parsed = ulong.TryParse(role.Id, out var roleId);
            if (!parsed) return BadRequest();
            roleIds.Add(roleId);
        }

        // Make sure all the roles exist in the guild and are below the bot's highest role.
        var botRole = guild.CurrentUser.Roles.Max(x => x.Position);
        var guildRoles = roleIds.Select(r => guild.GetRole(r)).Where(r => r is not null)
            .Where(r => r.Position < botRole && r is {IsManaged: false, IsEveryone: false})
            .ToArray();

        // If any of the roles are invalid, return bad request.
        if (guildRoles.Length != roleIds.Count) return BadRequest();

        // Get the public roles of the guild.
        var publicRoles = await _mediator.Send(new GetPublicRolesQuery
        {
            GuildId = guildIdParsed
        });

        // Make sure the requested roles are all public roles.
        if (roleIds.Any(roleId => !publicRoles.Contains(roleId))) return BadRequest();

        // Update the user's public roles.
        await _mediator.Send(new SetUserPublicRolesQuery
        {
            UserId = userId,
            GuildId = guildIdParsed,
            RoleIds = roleIds,
            PublicRoleIds = publicRoles
        });

        return Ok(guildRoles.Select(RoleDto.FromSocketRole).ToArray());
    }
}