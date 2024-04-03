using Domain.Models;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Configuration;

public class SetMinimumReportRoleCommand : IRequest
{
    public ulong GuildId { get; set; }

    /// <summary>
    ///     ID of the lowest role that can report messages. Null if everyone can use it.
    /// </summary>
    public ulong? RoleId { get; set; }
}

public class SetMinimumReportRoleCommandHandler : IRequestHandler<SetMinimumReportRoleCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public SetMinimumReportRoleCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SetMinimumReportRoleCommand request, CancellationToken cancellationToken)
    {
        var settings =
            await _dbContext.GuildSettings.FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken);
        if (settings is null)
        {
            await _dbContext.GuildSettings.AddAsync(new GuildSettings
            {
                GuildId = request.GuildId,
                MinimumReportRole = request.RoleId
            }, cancellationToken);
        }
        else
        {
            settings.MinimumReportRole = request.RoleId;
            _dbContext.GuildSettings.Update(settings);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}