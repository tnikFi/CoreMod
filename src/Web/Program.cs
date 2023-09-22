using Application;
using Application.Interfaces;
using Application.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure.Configuration;
using Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.RegisterRequestHandlers();

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

// Migrate the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Start the Discord client
var discordClient = app.Services.GetRequiredService<DiscordSocketClient>();
await discordClient.LoginAsync(TokenType.Bot, discordConfig.BotToken);
await discordClient.StartAsync();
await discordClient.SetStatusAsync(UserStatus.DoNotDisturb);

// Initialize the command handler for each scope
var commandHandlingService = app.Services.GetRequiredService<ICommandHandlingService>();
await commandHandlingService.InitializeAsync();
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