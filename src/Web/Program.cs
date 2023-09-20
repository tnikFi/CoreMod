using Discord;
using Discord.WebSocket;

var builder = WebApplication.CreateBuilder(args);

// Register appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Discord socket client
var discordSocketConfig = builder.Configuration.GetRequiredSection("Discord:SocketConfig").Get<DiscordSocketConfig>()
    ?? throw new InvalidOperationException("DiscordSocketConfig is null.");
builder.Services.AddSingleton(discordSocketConfig);
builder.Services.AddSingleton<DiscordSocketClient>();

var app = builder.Build();

// Start the Discord client
var discordClient = app.Services.GetRequiredService<DiscordSocketClient>();
await discordClient.LoginAsync(TokenType.Bot, app.Configuration["Discord:BotToken"]);
await discordClient.StartAsync();
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