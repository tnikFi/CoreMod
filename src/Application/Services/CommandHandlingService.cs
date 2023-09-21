using System.Reflection;
using Application.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure.Configuration;

namespace Application.Services;

public class CommandHandlingService : ICommandHandlingService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly DiscordConfiguration _discordConfig;

    public CommandHandlingService(IServiceProvider services, DiscordSocketClient client, CommandService commands, DiscordConfiguration discordConfig)
    {
        _commands = commands;
        _discordConfig = discordConfig;
        _client = client;
        _services = services;
        _commands.CommandExecuted += CommandExecutedAsync;
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage {Source: MessageSource.User} message)
            return;

        // Command prefix end index
        var argPos = 0;

        // Perform prefix check using the prefix in the configuration file.
        // Mention prefixes are also allowed.
        if (!message.HasStringPrefix(_discordConfig.DefaultPrefix, ref argPos)
            && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            return;

        // Create the websocket context
        var context = new SocketCommandContext(_client, message);

        // Perform the execution of the command. In this method, the command service will perform precondition
        // and parsing check, then execute the command if one is matched.
        await _commands.ExecuteAsync(context, argPos, _services);
    }

    private static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
        IResult result)
    {
        // Do nothing if a command wasn't found or execution was successful
        if (!command.IsSpecified || result.IsSuccess)
            return;

        // If a command failed, notify the user with the result
        await context.Channel.SendMessageAsync($"Error: {result}");
    }
}