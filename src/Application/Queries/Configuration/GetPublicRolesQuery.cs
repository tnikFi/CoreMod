using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Configuration;

/// <summary>
///     Get a list of public role IDs for a guild.
/// </summary>
public class GetPublicRolesQuery : IRequest<IEnumerable<ulong>>
{
    /// <summary>
    ///     ID of the guild to get the public roles for.
    /// </summary>
    public ulong GuildId { get; set; }
}

public class GetPublicRolesQueryHandler : IRequestHandler<GetPublicRolesQuery, IEnumerable<ulong>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetPublicRolesQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ulong>> Handle(GetPublicRolesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.PublicRoles
            .AsNoTracking()
            .Where(x => x.GuildId == request.GuildId)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);
    }
}