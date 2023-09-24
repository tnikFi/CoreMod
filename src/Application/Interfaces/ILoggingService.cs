using Discord;
using Discord.WebSocket;

namespace Application.Interfaces;

public interface ILoggingService
{
    /// <summary>
    ///     Log a message edit
    /// </summary>
    /// <param name="originalMessage">Original message if caching is enabled, otherwise the message ID</param>
    /// <param name="newMessage">Edited message</param>
    /// <param name="channel"></param>
    /// <returns></returns>
    Task LogMessageEdit(Cacheable<IMessage, ulong> originalMessage, SocketMessage newMessage,
        ISocketMessageChannel channel);

    /// <summary>
    ///     Log a single message deletion
    /// </summary>
    /// <param name="message">Deleted message if caching is enabled, otherwise the message ID</param>
    /// <param name="channel">Source channel</param>
    /// <returns></returns>
    Task LogMessageDelete(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel);

    /// <summary>
    ///     Log a bulk deletion of messages
    /// </summary>
    /// <param name="messages">Deleted messages if cached, otherwise the deleted message IDs</param>
    /// <param name="channel">Source channel</param>
    /// <returns></returns>
    Task LogBulkMessageDelete(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages,
        Cacheable<IMessageChannel, ulong> channel);

    /// <summary>
    ///     Log a custom moderation event
    /// </summary>
    /// <param name="guildId">Guild ID to log to</param>
    /// <param name="message">Message to log</param>
    /// <param name="embed">Embed to attach to the message</param>
    /// <param name="messageReference">Reference to a relevant message</param>
    /// <returns></returns>
    Task SendLogAsync(ulong guildId,
        string? message = null,
        Embed? embed = null,
        MessageReference? messageReference = null);
}