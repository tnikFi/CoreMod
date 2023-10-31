using Discord;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Queries.Moderation;

public class GetModerationQuery : IRequest<Domain.Models.Moderation?>
{
    /// <summary>
    ///     Guild in which the moderation action was performed.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     Id of the moderation action to retrieve.
    /// </summary>
    public required int Id { get; set; }
}

public class GetModerationQueryHandler : IRequestHandler<GetModerationQuery, Domain.Models.Moderation?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetModerationQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Domain.Models.Moderation?> Handle(GetModerationQuery request, CancellationToken cancellationToken)
    {
        var moderation = _dbContext.Moderations
            .FirstOrDefault(x => x.GuildId == request.Guild.Id && x.Id == request.Id);

        return Task.FromResult(moderation);
    }
}