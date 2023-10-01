using Application.Extensions;
using Application.Interfaces;
using Common.Utils;
using Discord;
using Discord.WebSocket;
using Domain.Attributes;
using Domain.Enums;
using Domain.Models;

namespace Application.Services;

/// <inheritdoc />
public class ModerationMessageService : IModerationMessageService
{
    private readonly DiscordSocketClient _client;
    private readonly ILoggingService _loggingService;

    public ModerationMessageService(DiscordSocketClient client, ILoggingService loggingService)
    {
        _client = client;
        _loggingService = loggingService;
    }

    /// <summary>
    ///     Send a message notifying a user of a moderation action and log it in the audit log channel for the guild if
    ///     it exists
    /// </summary>
    /// <param name="moderation"></param>
    /// <param name="sendLogMessage"></param>
    /// <returns></returns>
    public async Task<bool> SendModerationMessageAsync(Moderation moderation, bool sendLogMessage)
    {
        var user = await _client.GetUserAsync(moderation.UserId);
        var moderator = await _client.GetUserAsync(moderation.ModeratorId);
        var guild = _client.GetGuild(moderation.GuildId);

        // These shouldn't be null, but if they are, we can't send the message
        if (user is null || guild is null || moderator is null)
            return false;

        var dmEmbedBuilder = new EmbedBuilder()
            .WithTitle(GetDmEmbedTitleForModeration(moderation.Type, guild))
            .WithTimestamp(moderation.Timestamp)
            .AddField("Reason", moderation.Reason ?? "No reason provided.");

        var logEmbedBuilder = new EmbedBuilder()
            .WithAuthor(moderator)
            .WithTitle(GetLogEmbedTitleForModeration(moderation.Type))
            .AddField("User", user.Mention)
            .AddField("Moderator", moderator.Mention)
            .AddField("Reason", moderation.Reason ?? "No reason provided.")
            .WithTimestamp(moderation.Timestamp)
            .WithFooter($"Case #{moderation.Id}");

        // Add expiration if it's set
        if (moderation.ExpiresAt.HasValue)
        {
            const string title = "Expires";
            var timestampTag = new TimestampTag(moderation.ExpiresAt.Value, TimestampTagStyles.LongDateTime);
            dmEmbedBuilder.AddField(title, timestampTag);
            logEmbedBuilder.AddField(title, timestampTag);
        }

        // Set the embed color
        var color = EnumUtils.GetAttributeValue<EmbedColorAttribute>(moderation.Type)?.Color;
        if (color.HasValue)
        {
            dmEmbedBuilder.WithColor(color.Value);
            logEmbedBuilder.WithColor(color.Value);
        }

        // Send the message to the user
        var dmEmbed = dmEmbedBuilder.Build();
        var sentDm = await user.TrySendMessageAsync(embed: dmEmbed);

        // Log the moderation in the audit log channel
        if (sendLogMessage)
        {
            var logEmbed = logEmbedBuilder.Build();
            await _loggingService.SendLogAsync(moderation.GuildId, embed: logEmbed);
        }

        return sentDm != null;
    }

    /// <inheritdoc />
    public async Task<bool> SendBanExpirationMessageAsync(Moderation moderation, bool sendLogMessage)
    {
        var user = await _client.GetUserAsync(moderation.UserId);
        var guild = _client.GetGuild(moderation.GuildId);
        var moderator = guild.CurrentUser;

        // These shouldn't be null, but if they are, we can't send the message
        if (user is null || moderator is null)
            return false;

        var dmEmbedBuilder = new EmbedBuilder()
            .WithTitle(GetDmEmbedTitleForModeration(ModerationType.Unban, guild))
            .WithCurrentTimestamp();

        var logEmbedBuilder = new EmbedBuilder()
            .WithAuthor(moderator)
            .WithTitle(GetLogEmbedTitleForModeration(ModerationType.Unban))
            .AddField("User", user.Mention)
            .AddField("Moderator", moderator.Mention)
            .AddField("Reason", "Ban expired.")
            .WithTimestamp(moderation.Timestamp)
            .WithFooter($"Case #{moderation.Id}");

        // Add expiration if it's set
        if (moderation.ExpiresAt.HasValue)
        {
            const string title = "Expired at";
            var timestampTag = new TimestampTag(moderation.ExpiresAt.Value, TimestampTagStyles.LongDateTime);
            dmEmbedBuilder.AddField(title, timestampTag);
            logEmbedBuilder.AddField(title, timestampTag);
        }

        // Set the embed color
        var color = EnumUtils.GetAttributeValue<EmbedColorAttribute>(moderation.Type)?.Color;
        if (color.HasValue)
        {
            dmEmbedBuilder.WithColor(color.Value);
            logEmbedBuilder.WithColor(color.Value);
        }

        // Send the message to the user
        var dmEmbed = dmEmbedBuilder.Build();
        var sentDm = await user.TrySendMessageAsync(embed: dmEmbed);

        // Log the moderation in the audit log channel
        if (sendLogMessage)
        {
            var logEmbed = logEmbedBuilder.Build();
            await _loggingService.SendLogAsync(moderation.GuildId, embed: logEmbed);
        }

        return sentDm != null;
    }

    /// <summary>
    ///     Get the title for the moderation embed sent to the user
    /// </summary>
    /// <param name="moderationType">Type of moderation</param>
    /// <param name="guild">Guild the moderation is for</param>
    /// <returns>Embed title</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if moderation type is invalid</exception>
    private static string GetDmEmbedTitleForModeration(ModerationType moderationType, IGuild guild)
    {
        return moderationType switch
        {
            ModerationType.Warning => $"You were warned in {guild.Name}",
            ModerationType.Mute => $"You were muted in {guild.Name}",
            ModerationType.Kick => $"You were kicked from {guild.Name}",
            ModerationType.Ban => $"You were banned from {guild.Name}",
            ModerationType.Unmute => $"You were unmuted in {guild.Name}",
            ModerationType.Unban => $"You were unbanned from {guild.Name}",
            _ => throw new ArgumentOutOfRangeException(nameof(moderationType), moderationType, null)
        };
    }

    /// <summary>
    ///     Get the title for the embed sent to the audit log channel
    /// </summary>
    /// <param name="moderationType">Type of moderation</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if moderation type is invalid</exception>
    private static string GetLogEmbedTitleForModeration(ModerationType moderationType)
    {
        return moderationType switch
        {
            ModerationType.Warning => "User warned",
            ModerationType.Mute => "User muted",
            ModerationType.Kick => "User kicked",
            ModerationType.Ban => "User banned",
            ModerationType.Unmute => "User unmuted",
            ModerationType.Unban => "User unbanned",
            _ => throw new ArgumentOutOfRangeException(nameof(moderationType), moderationType, null)
        };
    }
}