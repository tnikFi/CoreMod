using Application.Interfaces;
using Application.Queries.Configuration;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public class LoggingService : ILoggingService
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;

    public LoggingService(IServiceProvider services, DiscordSocketClient client)
    {
        _services = services;
        _client = client;
    }

    /// <inheritdoc />
    public async Task LogMessageEdit(Cacheable<IMessage, ulong> originalMessage, SocketMessage newMessage,
        ISocketMessageChannel channel)
    {
        if (channel is not SocketTextChannel guildChannel || !ShouldLogMessage(newMessage))
            return;

        var embed = new EmbedBuilder()
            .WithAuthor(newMessage.Author)
            .WithDescription(
                $"**Message edited in {guildChannel.Mention}**\n[Jump to message]({newMessage.GetJumpUrl()})")
            .WithCurrentTimestamp()
            .AddField("Before", originalMessage.Value?.Content ?? "Unknown")
            .AddField("After", newMessage.Content ?? "Unknown")
            .Build();

        await SendLogAsync(guildChannel.Guild.Id, embed: embed);
    }

    /// <inheritdoc />
    public async Task LogMessageDelete(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
    {
        if (!channel.HasValue)
            return;

        if (channel.Value is not SocketTextChannel guildChannel || !ShouldLogMessage(message))
            return;

        var embed = new EmbedBuilder()
            .WithAuthor(message.Value.Author)
            .WithDescription($"**Message deleted in {guildChannel.Mention}**")
            .WithCurrentTimestamp()
            .AddField("Content", message.Value.Content ?? "Content unavailable")
            .Build();

        await SendLogAsync(guildChannel.Guild.Id, embed: embed);
    }

    /// <inheritdoc />
    public async Task LogBulkMessageDelete(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages,
        Cacheable<IMessageChannel, ulong> channel)
    {
        if (!channel.HasValue)
            return;

        if (channel.Value is not SocketTextChannel guildChannel)
            return;

        var embed = new EmbedBuilder()
            .WithDescription($"**Bulk message delete in {guildChannel.Mention}**")
            .WithCurrentTimestamp()
            .AddField("Message count", messages.Count)
            .Build();

        await SendLogAsync(guildChannel.Guild.Id, embed: embed);
    }

    /// <inheritdoc />
    public async Task SendLogAsync(ulong guildId,
        string? message = null,
        Embed? embed = null,
        MessageReference? messageReference = null)
    {
        // Get the log channel for this guild, don't log if there isn't one
        var logChannel = await GetLogChannel(guildId);
        if (logChannel is null)
            return;

        await logChannel.SendMessageAsync(message, embed: embed, messageReference: messageReference);
    }

    private static bool ShouldLogMessage(SocketMessage message)
    {
        // Don't log system messages
        if (message.Source != MessageSource.User)
            return false;

        // Don't log messages from bots
        if (message.Author.IsBot)
            return false;

        // Don't log messages from webhooks
        if (message.Author.IsWebhook)
            return false;

        // Only log messages in guilds
        return message.Channel is SocketTextChannel guildChannel;
    }

    private static bool ShouldLogMessage(Cacheable<IMessage, ulong> message)
    {
        if (message.Value is SocketMessage socketMessage) return ShouldLogMessage(socketMessage);

        // Don't log messages that aren't cached, since we can't check if they meet the criteria
        return false;
    }

    private async Task<SocketTextChannel?> GetLogChannel(ulong guildId)
    {
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var logChannelId = await mediator.Send(new GetLogChannelIdQuery {GuildId = guildId});
        if (logChannelId is null)
            return null;
        var channel = _client.GetChannel((ulong) logChannelId);
        return channel as SocketTextChannel;
    }
}