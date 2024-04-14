using Application.Commands.Configuration;
using Application.Queries.Configuration;
using Application.Queries.DiscordApiCalls;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure.Configuration;
using MediatR;

namespace Web.Discord.Modules;

[EnabledInDm(false)]
[RequireContext(ContextType.Guild)]
[Group("roles", "Manage public roles.")]
public class PublicRolesModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public PublicRolesModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Add a public role to the current user.
    /// </summary>
    /// <param name="role"></param>
    [SlashCommand("add", "Assign a public role to yourself.")]
    public async Task AddRoleAsync([Summary("role", "Public role to add")] IRole role)
    {
        if (Context.User is not IGuildUser user) return;
        await DeferAsync(true);

        if (!GetEligibleRoles().Contains(role))
        {
            await FollowupAsync("That role is not a public role.", ephemeral: true);
            return;
        }

        if (user.RoleIds.Contains(role.Id))
        {
            await FollowupAsync("You already have that role.", ephemeral: true);
            return;
        }

        var publicRoles = await GetPublicRolesAsync();
        if (!publicRoles.Contains(role))
        {
            await FollowupAsync("That role is not a public role.", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(role);
        await FollowupAsync($"You have been assigned the {role.Name} role.", ephemeral: true);
    }

    /// <summary>
    ///     Remove a public role from the current user.
    /// </summary>
    /// <param name="role"></param>
    [SlashCommand("remove", "Remove a public role from yourself.")]
    public async Task RemoveRoleAsync([Summary("role", "Public role to remove")] IRole role)
    {
        if (Context.User is not IGuildUser user) return;
        await DeferAsync(true);

        if (!GetEligibleRoles().Contains(role))
        {
            await FollowupAsync("That role is not a public role.", ephemeral: true);
            return;
        }

        if (!user.RoleIds.Contains(role.Id))
        {
            await FollowupAsync("You do not have that role.", ephemeral: true);
            return;
        }

        var publicRoles = await GetPublicRolesAsync();
        if (!publicRoles.Contains(role))
        {
            await FollowupAsync("That role is not a public role.", ephemeral: true);
            return;
        }

        await user.RemoveRoleAsync(role);
        await FollowupAsync($"The {role.Name} role has been removed.", ephemeral: true);
    }

    /// <summary>
    ///     List all public roles.
    /// </summary>
    [SlashCommand("list", "List all public roles.")]
    public async Task ListRolesAsync()
    {
        await DeferAsync(true);
        var publicRolesEnumerable = await GetPublicRolesAsync();
        var publicRoles = publicRolesEnumerable.ToArray();
        var publicRoleIds = publicRoles.Select(x => x.Id).ToArray();

        if (!publicRoles.Any())
        {
            await FollowupAsync("No public roles are configured on this server.", ephemeral: true);
            return;
        }

        // If the user does not have the Manage Roles permission, only show roles that the bot is able to assign.
        if (Context.User is IGuildUser {GuildPermissions.ManageRoles: false})
            publicRoles = publicRoles.Where(x => IsRoleEligible((SocketRole) x)).ToArray();

        var selectedPublicRoles = publicRoles.Intersect(GetUserPublicRoles((IGuildUser) Context.User, publicRoleIds))
            .ToArray();
        var otherPublicRoles = publicRoles.Except(selectedPublicRoles).ToArray();

        var embed = new EmbedBuilder()
            .WithTitle("Public Roles")
            .AddField("Your Public Roles",
                selectedPublicRoles.Length > 0
                    ? string.Join('\n', selectedPublicRoles.Select(x => x.Mention))
                    : "No roles", true)
            .AddField("Other Public Roles",
                otherPublicRoles.Length > 0 ? string.Join('\n', otherPublicRoles.Select(x => x.Mention)) : "No roles",
                true)
            .WithColor(Color.Blue)
            .Build();

        await FollowupAsync(embed: embed, ephemeral: true);
    }

    /// <summary>
    ///     Clear all public roles from the current user.
    /// </summary>
    [SlashCommand("clear", "Remove all public roles from yourself.")]
    public async Task ClearRolesAsync()
    {
        if (Context.User is not IGuildUser user) return;
        await DeferAsync(true);

        await _mediator.Send(new SetUserPublicRolesQuery
        {
            GuildId = Context.Guild.Id,
            UserId = user.Id,
            RoleIds = Array.Empty<ulong>()
        });

        await FollowupAsync("All public roles have been removed.", ephemeral: true);
    }

    /// <summary>
    ///     Get the public roles a user has.
    /// </summary>
    /// <param name="user">The user to get public roles for.</param>
    /// <param name="publicRoleIds">IDs of the guild's public roles.</param>
    /// <returns></returns>
    private IEnumerable<IRole> GetUserPublicRoles(IGuildUser user, IEnumerable<ulong> publicRoleIds)
    {
        return user.RoleIds
            .Where(publicRoleIds.Contains)
            .Select(x => user.Guild.GetRole(x));
    }

    /// <summary>
    ///     Get all roles in the current guild that are below the bot's highest role.
    ///     These roles can be used as public roles.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<IRole> GetEligibleRoles()
    {
        return Context.Guild.Roles.Where(IsRoleEligible);
    }

    /// <summary>
    ///     Check if a role is eligible to be a public role.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    private bool IsRoleEligible(SocketRole role)
    {
        var maxBotRole = Context.Guild.CurrentUser.Roles.Max(x => x.Position);
        return role is {IsManaged: false, IsEveryone: false} && role.Position < maxBotRole;
    }

    /// <summary>
    ///     Get the public roles for the current guild.
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<IRole>> GetPublicRolesAsync()
    {
        var publicRoles = await _mediator.Send(new GetPublicRolesQuery
        {
            GuildId = Context.Guild.Id
        });
        return publicRoles.Select(Context.Guild.GetRole).Where(x => x is not null);
    }

    [Group("config", "Configure public role settings.")]
    [RequireUserPermission(GuildPermission.ManageRoles)]
    public class PublicRoleConfigModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordConfiguration _discordConfiguration;
        private readonly IMediator _mediator;

        public PublicRoleConfigModule(IMediator mediator, DiscordConfiguration discordConfiguration)
        {
            _mediator = mediator;
            _discordConfiguration = discordConfiguration;
        }

        /// <summary>
        ///     Set a role as a public role for the guild.
        /// </summary>
        /// <param name="role"></param>
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [SlashCommand("add", "Set a role as a public role for the guild.")]
        public async Task SetPublicRoleAsync(
            [Summary("role", "The role to set as the public role.")]
            IRole role)
        {
            await DeferAsync(true);
            var botPosition = Context.Guild.CurrentUser.Hierarchy;

            if (role.Position >= botPosition)
            {
                await ModifyOriginalResponseAsync(
                    x => x.Content = "The role must be lower than the bot's highest role.");
                return;
            }

            var publicRoleIds = await _mediator.Send(new GetPublicRolesQuery
            {
                GuildId = Context.Guild.Id
            });
            var publicRoles = publicRoleIds.Select(id => Context.Guild.GetRole(id))
                .Where(x => x is {IsManaged: false, IsEveryone: false}).ToList();

            if (publicRoles.Contains(role))
            {
                await ModifyOriginalResponseAsync(x => x.Content = "The role is already a public role.");
                return;
            }

            if (publicRoles.Count >= _discordConfiguration.MaxPublicRoles)
            {
                await ModifyOriginalResponseAsync(x =>
                    x.Content = $"Cannot have more than {_discordConfiguration.MaxPublicRoles} public roles.");
                return;
            }

            var newPublicRoles = publicRoles.Append(role).Select(x => x.Id).ToArray();
            await _mediator.Send(new SetPublicRolesCommand
            {
                GuildId = Context.Guild.Id,
                RoleIds = newPublicRoles
            });

            await ModifyOriginalResponseAsync(x => x.Content = "The role has been set as a public role.");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [SlashCommand("remove", "Remove a role as a public role for the guild.")]
        public async Task RemovePublicRoleAsync(
            [Summary("role", "The role to remove as a public role.")]
            IRole role)
        {
            await DeferAsync(true);
            var publicRoleIds = await _mediator.Send(new GetPublicRolesQuery
            {
                GuildId = Context.Guild.Id
            });
            var publicRoles = publicRoleIds.Select(id => Context.Guild.GetRole(id))
                .Where(x => x is {IsManaged: false, IsEveryone: false}).ToList();

            if (!publicRoles.Contains(role))
            {
                await ModifyOriginalResponseAsync(x => x.Content = "The role is not a public role.");
                return;
            }

            var newPublicRoles = publicRoles.Where(x => x != role).Select(x => x.Id).ToArray();
            await _mediator.Send(new SetPublicRolesCommand
            {
                GuildId = Context.Guild.Id,
                RoleIds = newPublicRoles
            });

            await ModifyOriginalResponseAsync(x => x.Content = "The role has been removed as a public role.");
        }
    }
}