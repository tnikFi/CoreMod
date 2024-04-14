using Application.Queries.Configuration;
using Discord;
using MediatR;

namespace Application.Queries.DiscordApiCalls;

public class SetUserPublicRolesQuery : IRequest
{
    /// <summary>
    ///     User ID to set the roles for.
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    ///     Guild ID to set the roles in.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Role IDs to set for the user.
    /// </summary>
    public required IEnumerable<ulong> RoleIds { get; set; }

    /// <summary>
    ///     Public role IDs configured for the guild. If null, public roles will be fetched from the database.
    /// </summary>
    public IEnumerable<ulong>? PublicRoleIds { get; set; }
}

public class SetUserPublicRolesQueryHandler : IRequestHandler<SetUserPublicRolesQuery>
{
    private readonly IDiscordClient _discordClient;
    private readonly IMediator _mediator;

    public SetUserPublicRolesQueryHandler(IDiscordClient discordClient, IMediator mediator)
    {
        _discordClient = discordClient;
        _mediator = mediator;
    }

    public async Task Handle(SetUserPublicRolesQuery request, CancellationToken cancellationToken)
    {
        var guild = await _discordClient.GetGuildAsync(request.GuildId);
        if (guild is null) return;

        var user = await guild.GetUserAsync(request.UserId);
        if (user is null) return;

        var publicRoleIds = request.PublicRoleIds ??
                            await _mediator.Send(new GetPublicRolesQuery {GuildId = request.GuildId},
                                cancellationToken);

        // Get the public roles the user should have.
        var userPublicRoleIds = request.RoleIds.Where(id => publicRoleIds.Contains(id)).ToArray();

        // Get the roles to add and remove.
        var rolesToAdd = userPublicRoleIds.Where(role => !user.RoleIds.Contains(role)).ToArray();
        var rolesToRemove = publicRoleIds.Where(role => !request.RoleIds.Contains(role)).ToArray();

        if (rolesToAdd.Length > 0)
            await user.AddRolesAsync(rolesToAdd);
        
        if (rolesToRemove.Length > 0)
            await user.RemoveRolesAsync(rolesToRemove);
    }
}