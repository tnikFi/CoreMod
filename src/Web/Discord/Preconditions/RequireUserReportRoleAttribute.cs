using Application.Queries.Configuration;
using Discord;
using Discord.Interactions;
using MediatR;

namespace Web.Discord.Preconditions;

public class RequireUserReportRoleAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
        ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.User is not IGuildUser guildUser)
            return PreconditionResult.FromError("This command can only be used in a guild.");

        var mediator = services.GetRequiredService<IMediator>();
        var guildId = context.Guild.Id;
        var minimumReportRole = await mediator.Send(new GetMinimumReportRoleIdQuery
        {
            GuildId = guildId
        });

        // If no role is set, allow anyone to report
        // Also allow admins with any role to report
        if (minimumReportRole is null || guildUser.GuildPermissions.Administrator)
            return PreconditionResult.FromSuccess();

        // Find the role in the guild
        var role = guildUser.Guild.GetRole(minimumReportRole.Value);

        // If the role doesn't exist, assume it has been deleted and the report role hasn't been reconfigured.
        // In this case, only admins can use the command to prevent accidentally allowing everyone to report.
        if (role is null)
            return guildUser.GuildPermissions.Administrator
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("You do not have permission to use this feature.");

        // Check if the user has the required role or any role above it
        return guildUser.RoleIds.Any(x =>
            x == minimumReportRole.Value || guildUser.Guild.GetRole(x).Position >= role.Position)
            ? PreconditionResult.FromSuccess()
            : PreconditionResult.FromError("You do not have permission to use this feature.");
    }
}