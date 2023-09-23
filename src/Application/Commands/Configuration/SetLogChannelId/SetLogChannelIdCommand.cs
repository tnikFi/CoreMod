using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Configuration.SetLogChannelId;

public class SetLogChannelIdCommand : IRequest
{
    public ulong GuildId { get; set; }
    public ulong? LogChannelId { get; set; }
}

public class SetLogChannelIdCommandHandler : IRequestHandler<SetLogChannelIdCommand>
{
    private readonly ApplicationDbContext _dbContext;
    
    public SetLogChannelIdCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SetLogChannelIdCommand request, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.GuildSettings.FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken: cancellationToken);
        if (settings is null)
        {
            await _dbContext.GuildSettings.AddAsync(new Domain.Models.GuildSettings
            {
                GuildId = request.GuildId,
                LogChannelId = request.LogChannelId
            }, cancellationToken);
        }
        else
        {
            settings.LogChannelId = request.LogChannelId;
            _dbContext.GuildSettings.Update(settings);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}