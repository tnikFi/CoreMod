using System.Text;
using Application;
using Application.Interfaces;
using Application.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Configuration;
using Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddUserSecrets<Program>();

// Add logger.
builder.Services.AddLogging();

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseSnakeCaseNamingConvention();
});

builder.Services.RegisterRequestHandlers();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
var authConfig = builder.Configuration.GetRequiredSection("Authentication").Get<AuthConfiguration>()
                 ?? throw new InvalidOperationException("Authentication configuration is missing");
builder.Services.AddSingleton(authConfig);
builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.Jwt.SigningKey)),
            ValidateIssuerSigningKey = true,
            ValidAudience = authConfig.Jwt.Audience,
            ValidIssuer = authConfig.Jwt.Issuer
        };
    });

// Hangfire
builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(options =>
{
    options.UseNpgsqlConnection(connectionString);
}));
builder.Services.AddHangfireServer();

// Configure Discord
var discordConfig = builder.Configuration.GetRequiredSection("Discord").Get<DiscordConfiguration>()
                    ?? throw new InvalidOperationException("Discord configuration is missing");
discordConfig.SocketConfig.GatewayIntents =
    GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers;

builder.Services.AddSingleton(discordConfig);
builder.Services.AddSingleton(discordConfig.SocketConfig);
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<IDiscordClient>(x => x.GetRequiredService<DiscordSocketClient>());
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<InteractionService>();

builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<IModerationMessageService, ModerationMessageService>();
builder.Services.AddSingleton<ICommandHandlingService, CommandHandlingService>();
builder.Services.AddScoped<IUnbanSchedulingService, UnbanSchedulingService>();

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
discordClient.Ready += async () =>
{
    await commandHandlingService.InitializeAsync();
    await discordClient.SetStatusAsync(UserStatus.Online);
};

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

app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseCors(corsPolicyBuilder => corsPolicyBuilder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.UseAuthorization();

app.MapControllers();

app.Run();