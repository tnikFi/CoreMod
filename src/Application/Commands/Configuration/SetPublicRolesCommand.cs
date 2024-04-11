using Domain.Models;
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
    private readonly ApplicationDbContext _dbContext;

    public SetPublicRolesCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SetPublicRolesCommand request, CancellationToken cancellationToken)
    {
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