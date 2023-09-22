using Infrastructure.Configuration;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Configuration.GetCommandPrefix;

public class GetCommandPrefixQuery : IRequest<string>
{
    public ulong? GuildId { get; set; }
}

public class GetCommandPrefixQueryHandler : IRequestHandler<GetCommandPrefixQuery, string>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DiscordConfiguration _discordConfiguration;
    
    public GetCommandPrefixQueryHandler(ApplicationDbContext dbContext, DiscordConfiguration discordConfiguration)
    {
        _dbContext = dbContext;
        _discordConfiguration = discordConfiguration;
    }
    
    public async Task<string> Handle(GetCommandPrefixQuery request, CancellationToken cancellationToken)
    {
        if (request.GuildId is null) return _discordConfiguration.DefaultPrefix;
        var settings = await _dbContext.GuildSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken: cancellationToken);
        return settings?.CommandPrefix ?? _discordConfiguration.DefaultPrefix;
    }
}