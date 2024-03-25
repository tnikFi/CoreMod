using Application.Commands.Moderation;
using Application.Extensions;
using Application.Queries.Moderation;
using Common.Permissions;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GuildsController : BaseController
{
    public GuildsController(DiscordSocketClient discordClient, IMediator mediator) : base(discordClient, mediator) { }

    [HttpGet("{guildId}/member-count")]
    public Task<ActionResult<int>> GetGuildMemberCountAsync(string guildId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return Task.FromResult<ActionResult<int>>(BadRequest());
        var guild = DiscordClient.GetGuild(guildIdParsed);

        if (guild is null) return Task.FromResult<ActionResult<int>>(NotFound());

        return Task.FromResult<ActionResult<int>>(Ok(guild.MemberCount));
    }

    [HttpGet("{guildId}/channels")]
    [RequireGuildPermission(nameof(guildId), GuildPermission.ManageChannels)]
    public Task<ActionResult<ChannelDto[]>> GetGuildChannelsAsync(string guildId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed))
            return Task.FromResult<ActionResult<ChannelDto[]>>(BadRequest());
        var guild = DiscordClient.GetGuild(guildIdParsed);

        if (guild is null) return Task.FromResult<ActionResult<ChannelDto[]>>(NotFound());

        var user = GetCurrentGuildUser(guildIdParsed);

        if (user is null) return Task.FromResult<ActionResult<ChannelDto[]>>(Forbid());

        return Task.FromResult<ActionResult<ChannelDto[]>>(Ok(guild.Channels
            .Where(channel => channel is ITextChannel and not IVoiceChannel)
            .Where(channel => user.GetPermissions(channel).ViewChannel)
            .Select(ChannelDto.FromChannel)
            .ToArray()));
    }

    /// <summary>
    ///     Get basic information about a user in a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{guildId}/user/{userId}")]
    [RequireGuildPermission(nameof(guildId), CoreModPermissions.ViewModerations, true)]
    public async Task<ActionResult<UserDto>> GetUserAsync(string guildId, string userId)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed))
            return BadRequest();
        var guild = DiscordClient.GetGuild(guildIdParsed);

        if (guild is null) return NotFound();

        if (!ulong.TryParse(userId, out var userIdParsed)) return BadRequest();
        IUser user = guild.GetUser(userIdParsed);

        if (user is null)
        {
            // Check if the user is the bot itself.
            // If not, try to get the user from all known users and then the REST API.
            if (userIdParsed == DiscordClient.CurrentUser.Id)
                user = DiscordClient.CurrentUser;
            else if (DiscordClient.GetUser(userIdParsed) is IUser discordUser)
                user = discordUser;
            else if (await DiscordClient.Rest.GetUserAsync(userIdParsed) is IUser restUser)
                user = restUser;

            // If user is still null, return not found.
            if (user is null) return NotFound();
        }

        return Ok(UserDto.FromUser(user));
    }

    /// <summary>
    ///     Get a list of all the moderations in a guild.
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("{guildId}/moderations")]
    [RequireGuildPermission(nameof(guildId), CoreModPermissions.ViewModerations, true)]
    public async Task<ActionResult<PaginatedResult<ModerationDto>>> GetModerationsAsync(string guildId,
        [FromQuery] int page = 0, [FromQuery] int pageSize = 25)
    {
        // Return bad request if the page or page size is invalid.
        if (page < 0 || pageSize < 1) return BadRequest();
        if (pageSize > 100) return BadRequest("Page size cannot be greater than 100.");

        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed))
            return BadRequest();
        var guild = DiscordClient.GetGuild(guildIdParsed);

        if (guild is null) return NotFound();

        var moderations = await Mediator.Send(new GetModerationsQuery
        {
            Guild = guild
        });

        var totalItems = moderations.Count();
        var pageData = moderations.Paginate(page, pageSize)
            .Select(x => ModerationDto.FromDomainModel(x, true))
            .ToArray();

        return Ok(new PaginatedResult<ModerationDto>
        {
            Data = pageData.ToArray(),
            TotalItems = totalItems
        });
    }
    
    [HttpPatch("{guildId}/moderations/{moderationId}")]
    [RequireGuildPermission(nameof(guildId), GuildPermission.Administrator)]
    public async Task<ActionResult<ModerationDto>> UpdateModerationAsync(string guildId, int moderationId,
        [FromBody] ModerationDto request)
    {
        // Try to parse the guild ID and return bad request if it's invalid.
        if (!ulong.TryParse(guildId, out var guildIdParsed))
            return BadRequest();
        var guild = DiscordClient.GetGuild(guildIdParsed);

        if (guild is null) return NotFound();
        
        var moderation = await Mediator.Send(new GetModerationQuery
        {
            Guild = guild,
            Id = moderationId
        });

        if (moderation is null) return NotFound();

        moderation.Reason = request.Reason;
        moderation.ExpiresAt = request.ExpiresAt?.UtcDateTime;

        await Mediator.Send(new UpdateModerationCommand
        {
            Moderation = moderation
        });

        return Ok(ModerationDto.FromDomainModel(moderation, true));
    }
}