using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Configuration.GetWelcomeMessage;

public class GetWelcomeMessageQuery : IRequest<string?>
{
    public ulong GuildId { get; set; }
}

public class GetWelcomeMessageQueryHandler : IRequestHandler<GetWelcomeMessageQuery, string?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetWelcomeMessageQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> Handle(GetWelcomeMessageQuery request, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.GuildSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        return settings?.WelcomeMessage;
    }
}