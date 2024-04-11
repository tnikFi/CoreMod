using Application.Commands.Configuration;
using Application.Queries.Configuration;
using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Web.Discord.Modules;

[RequireContext(ContextType.Guild)]
public class PublicRolesModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string EditMyPublicRolesId = "edit_my_public_roles";
    private const string SaveMyRolesId = "save_my_public_roles";
    private const string ConfigurePublicRolesId = "configure_public_roles";
    private const string SavePublicRolesId = "save_public_roles";
    private readonly IMediator _mediator;

    public PublicRolesModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Entry point for the /roles command.
    /// </summary>
    [SlashCommand("roles", "Manage public roles.")]
    public async Task RolesAsync()
    {
        if (Context.User is not IGuildUser user) return;

        // If the user has the Manage Roles permission, they can manage public roles.
        // Show a dialog with options to configure public roles or to edit their own public roles.
        if (user.GuildPermissions.ManageRoles)
        {
            var configButton = new ButtonBuilder()
                .WithLabel("Configure Public Roles")
                .WithEmote(Emoji.Parse(":gear:"))
                .WithStyle(ButtonStyle.Primary)
                .WithCustomId(ConfigurePublicRolesId);

            var editButton = new ButtonBuilder()
                .WithLabel("Edit Your Public Roles")
                .WithEmote(Emoji.Parse(":pencil:"))
                .WithStyle(ButtonStyle.Secondary)
                .WithCustomId(EditMyPublicRolesId);

            var components = new ComponentBuilder()
                .WithButton(configButton)
                .WithButton(editButton)
                .Build();

            await RespondAsync("Select an option to manage public roles.", ephemeral: true, components: components);
            return;
        }

        await EditMyPublicRolesAsync();
    }

    /// <summary>
    ///     Edit the public roles for the current user.
    /// </summary>
    [ComponentInteraction(EditMyPublicRolesId)]
    public async Task EditMyPublicRolesAsync()
    {
        if (Context.User is not IGuildUser user) return;
        await DeferAsync(true);

        // If there are no eligible roles, don't bother querying the database.
        var eligibleRoles = GetEligibleRoles();
        var eligibleRoleIds = eligibleRoles.Select(x => x.Id).ToArray();
        if (!eligibleRoleIds.Any())
        {
            await FollowupAsync("No public roles are configured on this server.", ephemeral: true);
            return;
        }

        var publicRoles = await GetPublicRolesAsync();
        var publicRoleIds = publicRoles.Select(x => x.Id).ToArray();

        // If no public roles are configured, let the user know.
        if (!publicRoleIds.Any())
        {
            await ModifyOriginalResponseAsync(x => x.Content = "No public roles are configured on this server.");
            return;
        }

        // Only include public roles that are eligible.
        var selectableRoles = publicRoleIds.Intersect(eligibleRoleIds).ToArray();
        var userPublicRoles = GetUserPublicRoles(user, publicRoleIds);
        var selections = publicRoleIds.ToDictionary(x => x, x => userPublicRoles.Contains(Context.Guild.GetRole(x)));
        var selectMenu = BuildRoleSelect(SaveMyRolesId, selectableRoles, selections);
        var components = new ComponentBuilder()
            .WithSelectMenu(selectMenu)
            .Build();

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = "Select your public roles.";
            x.Components = components;
        });
    }

    /// <summary>
    ///     Save the public roles for the current user.
    /// </summary>
    [ComponentInteraction(SaveMyRolesId)]
    public async Task SaveMyRolesAsync()
    {
        if (Context.User is not IGuildUser user) return;
        await DeferAsync(true);
        var componentInteraction = (IComponentInteraction) Context.Interaction;
        var values = componentInteraction.Data.Values.Select(ulong.Parse).ToArray();
        var selectedRoles = values.Select(x => Context.Guild.GetRole(x)).ToArray();

        // Get a list of public roles for the guild.
        var publicRoles = await GetPublicRolesAsync();

        // Remove any roles that the user no longer has.
        var rolesToRemove = publicRoles.Where(x => !selectedRoles.Contains(x));
        foreach (var role in rolesToRemove)
            await user.RemoveRoleAsync(role);

        // Add any roles that the user does not have.
        var rolesToAdd = selectedRoles.Where(x => !user.RoleIds.Contains(x.Id));
        foreach (var role in rolesToAdd)
            await user.AddRoleAsync(role);

        await FollowupAsync("Your roles have been updated.", ephemeral: true);
    }

    /// <summary>
    ///     Public role configuration dialog handler.
    /// </summary>
    [ComponentInteraction(ConfigurePublicRolesId)]
    [DefaultMemberPermissions(GuildPermission.ManageRoles)]
    public async Task ConfigurePublicRolesAsync()
    {
        await DeferAsync(true);

        // Get all roles in the guild that are below the bot's highest role.
        var eligibleRoleIds = GetEligibleRoles().Select(x => x.Id).ToArray();

        var publicRoles = await GetPublicRolesAsync();
        var publicRoleIds = publicRoles.Select(x => x.Id).ToArray();

        // If no roles are eligible and no public roles are configured, let the user know.
        if (!eligibleRoleIds.Any())
        {
            await RespondAsync(
                "No roles are eligible to be public roles. Only roles below my highest role can be public roles.",
                ephemeral: true);
            return;
        }

        // Include all public roles in the selection even if they are not eligible to prevent ineligible roles from
        // being impossible to remove.
        var availableRoles = publicRoleIds.Concat(eligibleRoleIds).Distinct().ToArray();
        var selections = publicRoleIds.ToDictionary(x => x, x => true);
        var selectMenu = BuildRoleSelect(SavePublicRolesId, availableRoles, selections);
        var components = new ComponentBuilder()
            .WithSelectMenu(selectMenu)
            .Build();

        await FollowupAsync("Select the roles you would like to make public.", ephemeral: true, components: components);
    }

    /// <summary>
    ///     Save the guild's public roles.
    /// </summary>
    [ComponentInteraction(SavePublicRolesId)]
    [DefaultMemberPermissions(GuildPermission.ManageRoles)]
    public async Task SavePublicRolesAsync()
    {
        await DeferAsync(true);
        var componentInteraction = (IComponentInteraction) Context.Interaction;
        var values = componentInteraction.Data.Values.Select(ulong.Parse).ToArray();

        await _mediator.Send(new SetPublicRolesCommand
        {
            GuildId = Context.Guild.Id,
            RoleIds = values
        });

        await FollowupAsync("Public roles have been updated.", ephemeral: true);
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
    ///     Build a select menu for selecting public roles.
    /// </summary>
    /// <param name="customId">Custom ID for the select menu component.</param>
    /// <param name="roles">Roles to display in the select menu.</param>
    /// <param name="defaults">Default selections for the select menu.</param>
    /// <returns></returns>
    private SelectMenuBuilder BuildRoleSelect(string customId, IEnumerable<ulong> roles,
        IDictionary<ulong, bool> defaults)
    {
        var options = roles.Select(x =>
            {
                var role = Context.Guild.GetRole(x);
                var builder = new SelectMenuOptionBuilder()
                    .WithLabel(role.Name)
                    .WithValue(x.ToString())
                    .WithDefault(defaults.TryGetValue(x, out var selection) && selection);

                if (!role.Emoji.Name.IsNullOrEmpty())
                    builder = builder.WithEmote(role.Emoji);

                return builder;
            })
            .ToList();

        return new SelectMenuBuilder()
            .WithType(ComponentType.SelectMenu)
            .WithCustomId(customId)
            .WithMaxValues(options.Count)
            .WithMinValues(0)
            .WithOptions(options);
    }

    /// <summary>
    ///     Get all roles in the current guild that are below the bot's highest role.
    ///     These roles can be used as public roles.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<IRole> GetEligibleRoles()
    {
        var maxBotRole = Context.Guild.CurrentUser.Roles.Max(x => x.Position);
        return Context.Guild.Roles.Where(x => x.Position < maxBotRole && x is {IsManaged: false, IsEveryone: false});
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
}