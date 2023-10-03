using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Queries.Moderation;

public class GetModerationsQuery : IRequest<IQueryable<Domain.Models.Moderation>>
{
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     Type of moderation action to retrieve. If null, all moderation actions will be retrieved.
    /// </summary>
    public ModerationType? Type { get; set; }

    /// <summary>
    ///     User whose moderation actions should be retrieved. If null, all users' moderation actions for the guild will be
    ///     retrieved.
    /// </summary>
    public IUser? User { get; set; }

    /// <summary>
    ///     Moderator who performed the moderation action. If null, all moderators' moderation actions for the guild will be
    ///     retrieved.
    /// </summary>
    public IUser? Moderator { get; set; }
}

public class GetModerationsQueryHandler : IRequestHandler<GetModerationsQuery, IQueryable<Domain.Models.Moderation>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetModerationsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IQueryable<Domain.Models.Moderation>> Handle(GetModerationsQuery request,
        CancellationToken cancellationToken)
    {
        var moderations = _dbContext.Moderations
            .Where(x => x.GuildId == request.Guild.Id);

        if (request.User is not null) moderations = moderations.Where(x => x.UserId == request.User.Id);

        if (request.Type is not null) moderations = moderations.Where(x => x.Type == request.Type);

        if (request.Moderator is not null) moderations = moderations.Where(x => x.ModeratorId == request.Moderator.Id);

        return Task.FromResult(moderations);
    }
}