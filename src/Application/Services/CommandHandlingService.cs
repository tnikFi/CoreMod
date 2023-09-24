using System.Reflection;
using Application.Interfaces;
using Application.Queries.Configuration.GetCommandPrefix;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public class CommandHandlingService : ICommandHandlingService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services, DiscordSocketClient client, CommandService commands,
        ILoggingService loggingService)
    {
        _commands = commands;
        _client = client;
        _services = services;
        _commands.CommandExecuted += CommandExecutedAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.MessageUpdated += loggingService.LogMessageEdit;
        _client.MessageDeleted += loggingService.LogMessageDelete;
        _client.MessagesBulkDeleted += loggingService.LogBulkMessageDelete;
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

        // Get the guild-specific prefix via the GetCommandPrefixQuery
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        // Create a scope for the dependency injection
        using (var scope = _services.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var prefix = await mediator.Send(new GetCommandPrefixQuery {GuildId = guild?.Id});
            if (!message.HasStringPrefix(prefix, ref argPos)
                && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;
        }

        ;

        // Create the websocket context
        var context = new SocketCommandContext(_client, message);

        // Perform the execution of the command. In this method, the command service will perform precondition
        // and parsing check, then execute the command if one is matched.
        // Create a scope for the command in order to inject the dependencies like the db context and the mediator.
        using (var commandScope = _services.CreateScope())
        {
            await _commands.ExecuteAsync(context, argPos, commandScope.ServiceProvider);
        }
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