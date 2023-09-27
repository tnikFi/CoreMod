using Domain.Models;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Configuration.SetWelcomeMessage;

public class SetWelcomeMessageCommand : IRequest
{
    public ulong GuildId { get; set; }
    public string? WelcomeMessage { get; set; }
}

public class SetWelcomeMessageCommandHandler : IRequestHandler<SetWelcomeMessageCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public SetWelcomeMessageCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SetWelcomeMessageCommand request, CancellationToken cancellationToken)
    {
        var settings =
            await _dbContext.GuildSettings.FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        if (settings is null)
        {
            await _dbContext.GuildSettings.AddAsync(new GuildSettings
            {
                GuildId = request.GuildId,
                WelcomeMessage = request.WelcomeMessage
            }, cancellationToken);
        }
        else
        {
            settings.WelcomeMessage = request.WelcomeMessage;
            _dbContext.GuildSettings.Update(settings);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}