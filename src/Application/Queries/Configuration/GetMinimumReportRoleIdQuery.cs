using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Configuration;

public class GetMinimumReportRoleIdQuery : IRequest<ulong?>
{
    public ulong GuildId { get; set; }
}

public class GetMinimumReportRoleQueryHandler : IRequestHandler<GetMinimumReportRoleIdQuery, ulong?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMinimumReportRoleQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ulong?> Handle(GetMinimumReportRoleIdQuery request, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.GuildSettings
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        return settings?.MinimumReportRole;
    }
}