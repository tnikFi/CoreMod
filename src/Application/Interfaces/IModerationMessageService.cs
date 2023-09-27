using Domain.Models;

namespace Application.Interfaces;

/// <summary>
///     Provides notification functionality for moderation events
/// </summary>
public interface IModerationMessageService
{
    /// <summary>
    /// Send a moderation message
    /// </summary>
    /// <param name="moderation"></param>
    /// <param name="sendLogMessage">Whether or not to send the moderation info to audit logs</param>
    /// <returns>True if the message was sent to the user successfully</returns>
    public Task<bool> SendModerationMessageAsync(Moderation moderation, bool sendLogMessage = true);
}