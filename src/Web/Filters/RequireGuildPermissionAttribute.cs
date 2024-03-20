using Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class RequireGuildPermissionAttribute : ActionFilterAttribute
{
    private readonly string _guildIdArgumentName;
    private readonly GuildPermission _permission;

    public RequireGuildPermissionAttribute(string guildIdArgumentName, GuildPermission permission)
    {
        _permission = permission;
        _guildIdArgumentName = guildIdArgumentName;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var discordClient = context.HttpContext.RequestServices.GetRequiredService<IDiscordClient>();
        var userIdString = context.HttpContext.User.Claims.First(c => c.Type == "userId").Value;
        var userId = ulong.Parse(userIdString);
        var guildIdArgument = context.ActionArguments[_guildIdArgumentName];
        
        if (guildIdArgument is null)
        {
            context.Result = new BadRequestResult();
            return;
        }
        
        if (!ulong.TryParse(guildIdArgument.ToString(), out var guildId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var guild = await discordClient.GetGuildAsync(guildId);
        var user = await guild.GetUserAsync(userId);
        
        if (user.GuildPermissions.Has(_permission))
        {
            await next();
        }
        else
        {
            context.Result = new ForbidResult();
        }
    }
}