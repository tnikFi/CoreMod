﻿using Application.Commands.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure.Configuration;
using MediatR;

namespace Web.Discord.Modules;

[Name("Configuration")]
[Summary("Commands for configuring the bot")]
public class ConfigurationModule : ModuleBase<SocketCommandContext>
{
    private readonly DiscordConfiguration _discordConfiguration;
    private readonly IMediator _mediator;

    public ConfigurationModule(DiscordConfiguration discordConfiguration, IMediator mediator)
    {
        _discordConfiguration = discordConfiguration;
        _mediator = mediator;
    }

    [Command("prefix")]
    [Summary("Sets the command prefix.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task PrefixAsync([Summary("The new prefix")] string? prefix = null)
    {
        if (prefix == null || prefix.Length <= _discordConfiguration.MaxPrefixLength)
        {
            await _mediator.Send(new SetCommandPrefixCommand
            {
                GuildId = Context.Guild.Id,
                Prefix = prefix
            });
            await ReplyAsync($"Set the command prefix to `{prefix ?? _discordConfiguration.DefaultPrefix}`");
        }
        else
        {
            if (_discordConfiguration.MaxPrefixLength == 1)
                await ReplyAsync("The prefix can't be longer than 1 character.");
            else
                await ReplyAsync(
                    $"The prefix can't be longer than {_discordConfiguration.MaxPrefixLength} characters.");
        }
    }

    [Command("logchannel")]
    [Summary("Sets the moderation log channel.")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    public async Task SetLogChannelAsync(
        [Summary("The new log channel. Leave empty to disable logging.")]
        SocketTextChannel? channel = null)
    {
        await _mediator.Send(new SetLogChannelIdCommand
        {
            GuildId = Context.Guild.Id,
            LogChannelId = channel?.Id
        });
        if (channel is null)
            await ReplyAsync("Logging disabled.");
        else
            await ReplyAsync($"Set the log channel to {channel.Mention}.");
    }

    [Command("welcomemessage")]
    [Summary("Sets the welcome message sent to new users.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Alias("welcome")]
    [Remarks("Available placeholders:\n{user} - Mention the user who joined\n{guild} - The guild's name")]
    public async Task SetWelcomeMessageAsync(
        [Summary("Welcome message. Leave empty to disable.")] [Remainder]
        string? message = null)
    {
        await _mediator.Send(new SetWelcomeMessageCommand
        {
            GuildId = Context.Guild.Id,
            WelcomeMessage = message
        });
        if (message is null)
            await ReplyAsync("Welcome message disabled.");
        else
            await ReplyAsync($"Set the welcome message to:\n{message}");
    }

    [Command("reportchannel")]
    [Summary("Sets the user report channel.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Remarks("Leave empty to disable reporting.")]
    public async Task SetReportChannelAsync(
        [Summary("The new report channel.")] SocketTextChannel? channel = null)
    {
        await _mediator.Send(new SetReportChannelIdCommand
        {
            GuildId = Context.Guild.Id,
            ReportChannelId = channel?.Id
        });
        if (channel is null)
            await ReplyAsync("Reporting disabled.");
        else
            await ReplyAsync($"Set the report channel to {channel.Mention}.");
    }

    [Command("minreportrole")]
    [Summary("Sets the minimum role required to report messages.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Alias("minreport", "minimumreportrole", "reportrole")]
    [Remarks("Leave empty to allow everyone to report messages.")]
    public async Task SetMinimumReportRoleAsync(
        [Summary("The new minimum report role.")]
        SocketRole? role = null)
    {
        await _mediator.Send(new SetMinimumReportRoleCommand
        {
            GuildId = Context.Guild.Id,
            RoleId = role?.Id
        });
        if (role is null)
            await ReplyAsync("Everyone can now report messages.");
        else
            await ReplyAsync($"Set the minimum report role to {role.Name}.");
    }
}