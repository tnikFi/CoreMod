using Application.Interfaces;
using Application.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Discord
var discordConfig = builder.Configuration.GetRequiredSection("Discord").Get<DiscordConfiguration>()
    ?? throw new InvalidOperationException("Discord configuration is missing");
discordConfig.SocketConfig.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent;

builder.Services.AddSingleton(discordConfig);
builder.Services.AddSingleton(discordConfig.SocketConfig);
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();

builder.Services.AddSingleton<ICommandHandlingService, CommandHandlingService>();

var app = builder.Build();

// Start the Discord client
var discordClient = app.Services.GetRequiredService<DiscordSocketClient>();
await discordClient.LoginAsync(TokenType.Bot, discordConfig.BotToken);
await discordClient.StartAsync();
await discordClient.SetStatusAsync(UserStatus.DoNotDisturb);

// Initialize the command handler
await app.Services.GetRequiredService<ICommandHandlingService>().InitializeAsync();
await discordClient.SetStatusAsync(UserStatus.Online);

// Make sure the bot logs out instantly when the app stops
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    // Stop the Discord client
    discordClient.SetStatusAsync(UserStatus.DoNotDisturb).Wait();
    discordClient.LogoutAsync().Wait();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();