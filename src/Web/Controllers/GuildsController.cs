using Discord;
using Discord.WebSocket;
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
    public GuildsController(DiscordSocketClient discordClient) : base(discordClient) { }
    
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
        if (!ulong.TryParse(guildId, out var guildIdParsed)) return Task.FromResult<ActionResult<ChannelDto[]>>(BadRequest());
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
}