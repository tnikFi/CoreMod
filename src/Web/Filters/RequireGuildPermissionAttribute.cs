using Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class RequireGuildPermissionAttribute : ActionFilterAttribute
{
    private readonly bool _allowPartial;
    private readonly string _guildIdArgumentName;
    private readonly GuildPermission _permission;

    /// <summary>
    ///     Require a specific permission in a guild.
    /// </summary>
    /// <param name="guildIdArgumentName">Name of the argument that contains the guild ID.</param>
    /// <param name="permission">The permission(s) to require.</param>
    /// <param name="allowPartial">
    ///     Whether to allow partial permissions. If <see langword="false" /> (default), having all set permission flags
    ///     is required. If <see langword="true" />, having any of the set permission flags is enough.
    /// </param>
    public RequireGuildPermissionAttribute(string guildIdArgumentName, GuildPermission permission,
        bool allowPartial = false)
    {
        _permission = permission;
        _guildIdArgumentName = guildIdArgumentName;
        _allowPartial = allowPartial;
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
            await next();
        // If we don't require all permissions, we can just check if the bitwise AND of the user's permissions and the
        // required permissions is not 0.
        else if (_allowPartial && (user.GuildPermissions.RawValue & (ulong) _permission) != 0)
            await next();
        else
            context.Result = new ForbidResult();
    }
}