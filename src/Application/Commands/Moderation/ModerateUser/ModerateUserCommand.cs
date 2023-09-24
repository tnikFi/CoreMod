using Discord;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation.ModerateUser;

public class ModerateUserCommand : IRequest<int>
{
    public IGuild Guild { get; set; }
    public IGuildUser User { get; set; }
    public IGuildUser Moderator { get; set; }
    public string? Reason { get; set; }
    public ModerationType Type { get; set; }
}

public class ModerateUserCommandHandler : IRequestHandler<ModerateUserCommand, int>
{
    private readonly ApplicationDbContext _dbContext;

    public ModerateUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(ModerateUserCommand request, CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = request.Type
        };

        // Add the moderation and return the case number
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return moderation.Id;
    }
}