using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Configuration;

public class GetLogChannelIdQuery : IRequest<ulong?>
{
    public ulong GuildId { get; set; }
}

public class GetLogChannelQueryIdHandler : IRequestHandler<GetLogChannelIdQuery, ulong?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetLogChannelQueryIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ulong?> Handle(GetLogChannelIdQuery request, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.GuildSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        return settings?.LogChannelId;
    }
}