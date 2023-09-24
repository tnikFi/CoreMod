using Discord;
using Discord.Net;

namespace Common.Extensions;

public static class UserExtensions
{
    /// <summary>Tries to send a message via DM.</summary>
    /// <param name="user">The user to send the DM to.</param>
    /// <param name="text">The message to be sent.</param>
    /// <param name="isTTS">Whether the message should be read aloud by Discord or not.</param>
    /// <param name="embed">The <see cref="F:Discord.EmbedType.Rich" /> <see cref="T:Discord.Embed" /> to be sent.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <param name="allowedMentions">
    ///     Specifies if notifications are sent for mentioned users and roles in the message <paramref name="text" />.
    ///     If <see langword="null" />, all mentioned roles and users will be notified.
    /// </param>
    /// <param name="components">The message components to be included with this message. Used for interactions.</param>
    /// <param name="embeds">A array of <see cref="T:Discord.Embed" />s to send with this response. Max 10.</param>
    /// <returns>
    ///     A task that represents the asynchronous send operation. The task result contains the sent message if
    ///     the message was sent successfully, otherwise <see langword="null" />.
    /// </returns>
    public static async Task<IUserMessage?> TrySendMessageAsync(
        this IUser user,
        string text = null,
        bool isTTS = false,
        Embed embed = null,
        RequestOptions options = null,
        AllowedMentions allowedMentions = null,
        MessageComponent components = null,
        Embed[] embeds = null)
    {
        try
        {
            return await user.SendMessageAsync(text, isTTS, embed, options, allowedMentions, components, embeds);
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            return null;
        }

        return null;
    } 
}