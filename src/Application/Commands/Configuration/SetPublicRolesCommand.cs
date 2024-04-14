using Discord;
using Domain.Models;
using Infrastructure.Configuration;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Configuration;

/// <summary>
///     Set the public roles for a guild. Replaces any existing public roles.
/// </summary>
public class SetPublicRolesCommand : IRequest
{
    /// <summary>
    ///     ID of the guild to set the public roles for.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Role IDs to set as public roles.
    /// </summary>
    public IEnumerable<ulong>? RoleIds { get; set; }
}

public class SetPublicRolesCommandHandler : IRequestHandler<SetPublicRolesCommand>
{
    private readonly IDiscordClient _client;
    private readonly ApplicationDbContext _dbContext;
    private readonly DiscordConfiguration _discordConfiguration;

    public SetPublicRolesCommandHandler(ApplicationDbContext dbContext, IDiscordClient client,
        DiscordConfiguration discordConfiguration)
    {
        _dbContext = dbContext;
        _client = client;
        _discordConfiguration = discordConfiguration;
    }

    public async Task Handle(SetPublicRolesCommand request, CancellationToken cancellationToken)
    {
        // Limit the number of public roles.
        if (request.RoleIds?.Count() > _discordConfiguration.MaxPublicRoles)
            throw new InvalidOperationException(
                $"Cannot have more than {_discordConfiguration.MaxPublicRoles} public roles.");

        // Get the corresponding public roles for the guild.
        var guild = await _client.GetGuildAsync(request.GuildId);
        if (guild is null)
            return;

        // Throw an exception if any of the roles is not found or is managed.
        if (request.RoleIds is not null && request.RoleIds.Any(x => guild.GetRole(x) is null or {IsManaged: true}))
            throw new InvalidOperationException("One or more roles are invalid.");

        var existingRoles = await _dbContext.PublicRoles
            .Where(x => x.GuildId == request.GuildId)
            .ToListAsync(cancellationToken);

        _dbContext.PublicRoles.RemoveRange(existingRoles);

        if (request.RoleIds is not null)
        {
            var newRoles = request.RoleIds.Select(x => new PublicRole
            {
                GuildId = request.GuildId,
                RoleId = x
            });
            await _dbContext.PublicRoles.AddRangeAsync(newRoles, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}