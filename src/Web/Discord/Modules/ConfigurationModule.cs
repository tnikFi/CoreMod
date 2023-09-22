﻿using Application.Commands.Configuration.SetCommandPrefix;
using Discord;
using Discord.Commands;
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
            {
                await ReplyAsync("The prefix can't be longer than 1 character.");
            }
            else
            {
                await ReplyAsync(
                    $"The prefix can't be longer than {_discordConfiguration.MaxPrefixLength} characters.");
            }
        }
    }
}