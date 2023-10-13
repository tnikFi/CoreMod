using System.Reflection;
using Application.Interfaces;
using Application.Queries.Configuration;
using Application.Queries.DiscordApiCalls;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure.Configuration;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IResult = Discord.Commands.IResult;

namespace Application.Services;

public class CommandHandlingService : ICommandHandlingService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly DiscordConfiguration _discordConfig;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services, DiscordSocketClient client, CommandService commands,
        ILoggingService loggingService, InteractionService interactionService, DiscordConfiguration discordConfig)
    {
        _commands = commands;
        _interactionService = interactionService;
        _discordConfig = discordConfig;
        _client = client;
        _services = services;
        _commands.CommandExecuted += CommandExecutedAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.MessageUpdated += loggingService.LogMessageEdit;
        _client.MessageDeleted += loggingService.LogMessageDelete;
        _client.MessagesBulkDeleted += loggingService.LogBulkMessageDelete;
        _client.UserJoined += HandleJoinAsync;
        _client.InteractionCreated += InteractionCreatedAsync;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
#if DEBUG
        if (_discordConfig.DebugGuildId.HasValue)
            await _interactionService.RegisterCommandsToGuildAsync(_discordConfig.DebugGuildId.Value);
#else
        await _interactionService.RegisterCommandsGloballyAsync();
#endif
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

    private async Task InteractionCreatedAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        var scope = _services.CreateScope();
        var result = await _interactionService.ExecuteCommandAsync(context, scope.ServiceProvider);
        if (!result.IsSuccess)
            await context.Channel.SendMessageAsync($"Error: {result}");
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

    private async Task HandleJoinAsync(IGuildUser user)
    {
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new SendWelcomeMessageQuery
        {
            User = user
        });
    }
}