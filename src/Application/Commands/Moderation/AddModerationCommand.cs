using Common.Utils;
using Discord;
using Domain.Attributes;
using Domain.Enums;
using Infrastructure.Data.Contexts;
using MediatR;

namespace Application.Commands.Moderation;

/// <summary>
///     Add a moderation to the database.
/// </summary>
/// <remarks>
///     Does not apply the moderation to the user. See the other moderation commands in the same namespace for that.
/// </remarks>
public class AddModerationCommand : IRequest<Domain.Models.Moderation>
{
    /// <summary>
    ///     Guild to add the moderation to.
    /// </summary>
    public required IGuild Guild { get; set; }

    /// <summary>
    ///     User to add the moderation for.
    /// </summary>
    public required IUser User { get; set; }

    /// <summary>
    ///     Moderator adding the moderation.
    /// </summary>
    public required IUser Moderator { get; set; }

    /// <summary>
    ///     Reason for the moderation.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    ///     Type of moderation.
    /// </summary>
    public required ModerationType Type { get; set; }

    /// <summary>
    ///     Duration of the moderation.
    /// </summary>
    public TimeSpan? Duration { get; set; }
}

public class AddModerationCommandHandler : IRequestHandler<AddModerationCommand, Domain.Models.Moderation>
{
    private readonly ApplicationDbContext _dbContext;

    public AddModerationCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Models.Moderation> Handle(AddModerationCommand request,
        CancellationToken cancellationToken)
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = request.Guild.Id,
            UserId = request.User.Id,
            ModeratorId = request.Moderator.Id,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
            Type = request.Type
        };

        // Set the expiration date if the moderation type allows it and a duration was provided
        if (EnumUtils.HasAttribute<CanBeTemporaryAttribute>(moderation.Type) && request.Duration.HasValue)
            moderation.ExpiresAt = DateTime.UtcNow + request.Duration.Value;

        // Add the moderation to the database
        _dbContext.Moderations.Add(moderation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return moderation;
    }
}