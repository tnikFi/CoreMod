using Domain.Models;
using Infrastructure.Configuration;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Configuration.SetCommandPrefix;

public class SetCommandPrefixCommand : IRequest
{
    public ulong GuildId { get; set; }
    public string? Prefix { get; set; }
}

public class SetCommandPrefixCommandHandler : IRequestHandler<SetCommandPrefixCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DiscordConfiguration _discordConfiguration;
    
    public SetCommandPrefixCommandHandler(ApplicationDbContext dbContext, DiscordConfiguration discordConfiguration)
    {
        _dbContext = dbContext;
        _discordConfiguration = discordConfiguration;
    }

    public async Task Handle(SetCommandPrefixCommand request, CancellationToken cancellationToken)
    {
        var settings = _dbContext.GuildSettings.FirstOrDefault(x => x.GuildId == request.GuildId);
        if (settings is null)
        {
            settings = new GuildSettings
            {
                GuildId = request.GuildId,
                CommandPrefix = request.Prefix ?? _discordConfiguration.DefaultPrefix
            };
            _dbContext.GuildSettings.Add(settings);
        }
        else
        {
            settings.CommandPrefix = request.Prefix ?? _discordConfiguration.DefaultPrefix;
            _dbContext.GuildSettings.Update(settings);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}