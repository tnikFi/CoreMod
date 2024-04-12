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
using Microsoft.Extensions.Logging;
using IResult = Discord.Commands.IResult;

namespace Application.Services;

public class CommandHandlingService : ICommandHandlingService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly DiscordConfiguration _discordConfig;
    private readonly InteractionService _interactionService;
    private readonly ILogger<CommandHandlingService> _logger;
    private readonly ILoggingService _loggingService;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services, DiscordSocketClient client, CommandService commands,
        ILoggingService loggingService, InteractionService interactionService, DiscordConfiguration discordConfig,
        ILogger<CommandHandlingService> logger)
    {
        _commands = commands;
        _loggingService = loggingService;
        _interactionService = interactionService;
        _discordConfig = discordConfig;
        _logger = logger;
        _client = client;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
#if DEBUG
        if (_discordConfig.DebugGuildId.HasValue)
        {
            await _interactionService.RemoveModulesFromGuildAsync(_discordConfig.DebugGuildId.Value);
            await _interactionService.RegisterCommandsToGuildAsync(_discordConfig.DebugGuildId.Value);
            _logger.LogInformation("Registered commands to debug guild.");
        }
        else
        {
            _logger.LogWarning("Debug guild ID is not set. Interaction commands will be unavailable.");
        }
#else
        await _interactionService.RegisterCommandsGloballyAsync();
        _logger.LogInformation("Registered commands globally.");
#endif
        _commands.CommandExecuted += CommandExecutedAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.MessageUpdated += _loggingService.LogMessageEdit;
        _client.MessageDeleted += _loggingService.LogMessageDelete;
        _client.MessagesBulkDeleted += _loggingService.LogBulkMessageDelete;
        _client.UserJoined += HandleJoinAsync;
        _client.InteractionCreated += InteractionCreatedAsync;
        _interactionService.ContextCommandExecuted += ContextCommandExecutedAsync;
        _interactionService.InteractionExecuted += InteractionExecutedAsync;
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

            // If the message only contained a mention of the bot, reply with the current prefix.
            if (message.Content == $"<@{_client.CurrentUser.Id}>")
            {
                await message.ReplyAsync(
                    $"The current prefix for this server is `{prefix}`");
                return;
            }

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

    /// <summary>
    ///     Log exceptions that occur during command handling.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="context"></param>
    /// <param name="result"></param>
    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context,
        IResult result)
    {
        // Do nothing if a command wasn't found or execution was successful
        if (!command.IsSpecified || result.IsSuccess)
            return;

        var commandInfo = command.Value;

        if (result.Error == CommandError.Exception)
        {
            var commandLocation =
                $"Command \"{commandInfo.Name}\" handled by {commandInfo.Module.Name}";
            _logger.LogError(
                "Exception was thrown while executing command in message {MessageId}: {Error}\n\t{Location}",
                context.Message.Id, result.ErrorReason, commandLocation);
        }

        // If a command failed, notify the user with the result
        await context.Channel.SendMessageAsync($"Error: {result}");
    }

    private static async Task ContextCommandExecutedAsync(ContextCommandInfo command, IInteractionContext context,
        Discord.Interactions.IResult result)
    {
        // Do nothing if a command wasn't found or execution was successful
        if (result.IsSuccess)
            return;

        // If a command failed, notify the user with the result
        switch (result.Error)
        {
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.ConvertFailed:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                await context.Interaction.RespondAsync("An error occurred while executing the command.",
                    ephemeral: true);
                break;
            case InteractionCommandError.Unsuccessful:
                await context.Interaction.RespondAsync("The command was not successful.", ephemeral: true);
                break;
            case InteractionCommandError.UnmetPrecondition:
                await context.Interaction.RespondAsync(result.ErrorReason ?? "This command cannot be executed here.",
                    ephemeral: true);
                break;
            case InteractionCommandError.ParseFailed:
                break;
            case null:
                break;
        }
    }

    /// <summary>
    ///     Log exceptions that occur during interaction handling.
    /// </summary>
    /// <param name="commandInfo"></param>
    /// <param name="context"></param>
    /// <param name="result"></param>
    private async Task InteractionExecutedAsync(ICommandInfo commandInfo, IInteractionContext context,
        Discord.Interactions.IResult result)
    {
        // Do nothing if a command wasn't found or execution was successful
        if (result.IsSuccess)
            return;

        // If a command failed, notify the user with the result
        switch (result.Error)
        {
            case InteractionCommandError.Exception:
                var commandLocation =
                    $"Command \"{commandInfo.Name}\" handled by {commandInfo.Module.Name}.{commandInfo.MethodName}";
                _logger.LogError(
                    "Exception was thrown while executing interaction {Interaction}: {Error}\n\t{Location}",
                    context.Interaction.Id, result.ErrorReason, commandLocation);
                await context.Interaction.FollowupAsync(
                    $"An error occurred while executing the command. (Interaction ID {context.Interaction.Id})",
                    ephemeral: true);
                break;
            case null:
                break;
        }
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