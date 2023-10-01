using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces;

public interface IUnbanSchedulingService
{
    /// <summary>
    ///     Schedule a task to unban a user after a moderation expires.
    /// </summary>
    /// <param name="moderation">Ban moderation.</param>
    /// <remarks>
    ///     The moderation's <see cref="Moderation.ExpiresAt" /> property should be set and the <see cref="Moderation.Type" />
    ///     should be <see cref="ModerationType.Ban" />. The moderation should also be active.
    /// </remarks>
    public void ScheduleBanExpiration(Moderation moderation);
}