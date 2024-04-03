using Discord;
using Domain.Models;

namespace Application.Interfaces;

/// <summary>
///     Provides notification functionality for moderation events
/// </summary>
public interface IModerationMessageService
{
    /// <summary>
    ///     Send a moderation message
    /// </summary>
    /// <param name="moderation"></param>
    /// <param name="sendLogMessage">Whether or not to send the moderation info to audit logs</param>
    /// <returns>True if the message was sent to the user successfully</returns>
    public Task<bool> SendModerationMessageAsync(Moderation moderation, bool sendLogMessage = true);

    /// <summary>
    ///     Send a message notifying a user that their ban has expired
    /// </summary>
    /// <param name="moderation">Expired ban moderation</param>
    /// <param name="sendLogMessage">Whether or not to send the moderation info to audit logs</param>
    /// <returns>True if the message was sent to the user successfully</returns>
    public Task<bool> SendBanExpirationMessageAsync(Moderation moderation, bool sendLogMessage = true);
    
    /// <summary>
    ///     Send a message notifying that a moderation action has been deleted
    /// </summary>
    /// <param name="moderation">Deleted moderation</param>
    /// <param name="deletedBy">User who deleted the moderation</param>
    /// <param name="sendDirectMessage">Whether or not to notify the user directly</param>
    /// <returns>True if the message was sent to the user successfully</returns>
    public Task<bool> SendModerationDeletedMessageAsync(Moderation moderation, IGuildUser? deletedBy, bool sendDirectMessage = true);
}