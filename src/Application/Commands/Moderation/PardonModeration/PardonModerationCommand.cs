using Application.Commands.Moderation.ModerateUser;
using Common.Utils;
using Discord;
using Domain.Attributes;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation.PardonModeration;

/// <summary>
///     Pardon a moderation. The request will return the new moderation instance that was created to pardon the original
///     moderation.
/// </summary>
public class PardonModerationCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Moderation to pardon.
    /// </summary>
    public required Domain.Models.Moderation Moderation { get; init; }

    public required IGuildUser Moderator { get; init; }
    public string? Reason { get; init; }
}

public class PardonModerationCommandHandler : IRequestHandler<PardonModerationCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDiscordClient _discordClient;
    private readonly IMediator _mediator;

    public PardonModerationCommandHandler(ApplicationDbContext dbContext, IMediator mediator,
        IDiscordClient discordClient)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _discordClient = discordClient;
    }

    public async Task<Domain.Models.Moderation> Handle(PardonModerationCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.Moderation.Active)
            throw new InvalidOperationException("Moderation is not active.");

        var guild = await _discordClient.GetGuildAsync(request.Moderation.GuildId);
        var user = await guild.GetUserAsync(request.Moderation.UserId);
        var type = EnumUtils.GetValuesWithAttribute<ModerationType, PardonAttribute>()
            .FirstOrDefault(x =>
                EnumUtils.GetAttributeValue<PardonAttribute>(x)?.PardonedType == request.Moderation.Type);

        if (type is 0)
            throw new InvalidOperationException("The moderation type can not be pardoned.");

        // Throw an exception if any of the required values are null
        if (guild is null)
            throw new InvalidOperationException("Guild not found.");
        if (user is null)
            throw new InvalidOperationException("User not found.");

        var expiration = await _mediator.Send(new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = request.Moderator,
            Type = type,
            Reason = request.Reason
        }, cancellationToken);
        expiration.RelatedCase = request.Moderation;
        _dbContext.Moderations.Update(expiration);

        request.Moderation.RelatedCase = expiration;
        _dbContext.Moderations.Update(request.Moderation);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return expiration;
    }
}