using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Configuration;

public class GetReportChannelIdQuery : IRequest<ulong?>
{
    public ulong GuildId { get; set; }
}

public class GetReportChannelQueryIdHandler : IRequestHandler<GetReportChannelIdQuery, ulong?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetReportChannelQueryIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ulong?> Handle(GetReportChannelIdQuery request, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.GuildSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        return settings?.ReportChannelId;
    }
}