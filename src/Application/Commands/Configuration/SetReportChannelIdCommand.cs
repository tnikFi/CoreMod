using Domain.Models;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Configuration;

public class SetReportChannelIdCommand : IRequest
{
    public ulong GuildId { get; set; }
    public ulong? ReportChannelId { get; set; }
}

public class SetReportChannelIdCommandHandler : IRequestHandler<SetReportChannelIdCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public SetReportChannelIdCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SetReportChannelIdCommand request, CancellationToken cancellationToken)
    {
        var settings =
            await _dbContext.GuildSettings.FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        if (settings is null)
        {
            await _dbContext.GuildSettings.AddAsync(new GuildSettings
            {
                GuildId = request.GuildId,
                ReportChannelId = request.ReportChannelId
            }, cancellationToken);
        }
        else
        {
            settings.ReportChannelId = request.ReportChannelId;
            _dbContext.GuildSettings.Update(settings);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}