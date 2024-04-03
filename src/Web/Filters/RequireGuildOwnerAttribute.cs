using Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class RequireGuildOwnerAttribute : ActionFilterAttribute
{
    private readonly string _guildIdArgumentName;

    /// <summary>
    ///     Require the user to be the owner of the guild.
    /// </summary>
    /// <param name="guildIdArgumentName">Name of the argument that contains the guild ID.</param>
    public RequireGuildOwnerAttribute(string guildIdArgumentName)
    {
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
        
        if (guild is null)
        {
            context.Result = new NotFoundResult();
            return;
        }
        
        if (guild.OwnerId == userId)
            await next();
        else
            context.Result = new ForbidResult();
    }
}