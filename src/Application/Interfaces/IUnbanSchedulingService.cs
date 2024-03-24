using Domain.Enums;
using Domain.Models;

namespace Application.Interfaces;

public interface IUnbanSchedulingService
{
    /// <summary>
    ///     Schedule or reschedule a task to unban a user after a moderation expires.
    /// </summary>
    /// <param name="moderation">Ban moderation.</param>
    /// <remarks>
    ///     The moderation's <see cref="Moderation.ExpiresAt" /> property should be set and the <see cref="Moderation.Type" />
    ///     should be <see cref="ModerationType.Ban" />. The moderation should also be active.
    /// </remarks>
    /// <returns>The Hangfire job ID for the scheduled task.</returns>
    public string ScheduleBanExpiration(Moderation moderation);

    /// <summary>
    ///     Reschedule the expiration of a ban.
    /// </summary>
    /// <param name="moderation"></param>
    /// <returns><see langword="true" /> if the job was successfully rescheduled.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the moderation is not a ban, does not have an associated scheduled unban job, is not active, or
    ///     the new expiration time is in the past.
    /// </exception>
    public bool UpdateBanExpiration(Moderation moderation);

    /// <summary>
    ///     Cancel the expiration of a ban.
    /// </summary>
    /// <param name="moderation"></param>
    /// <returns><see langword="true" /> if the job was successfully cancelled.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the moderation is not a ban, is not active or does not have an associated scheduled unban job.
    /// </exception>
    public bool CancelBanExpiration(Moderation moderation);
}