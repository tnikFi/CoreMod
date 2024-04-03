using Application;
using Application.Interfaces;
using Discord;
using Discord.WebSocket;
using Domain.Models;
using Infrastructure.Configuration;
using Infrastructure.Data.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Integration.Tests;

/// <summary>
///     Handles the setup for the integration tests.
/// </summary>
public class TestBase
{
    private IMediator _mediator;
    private IServiceProvider _serviceProvider;
    protected IDiscordClient DiscordClient => _serviceProvider.GetRequiredService<IDiscordClient>();

    protected IUnbanSchedulingService UnbanSchedulingService =>
        _serviceProvider.GetRequiredService<IUnbanSchedulingService>();

    protected ApplicationDbContext TestDbContext => _serviceProvider.GetRequiredService<ApplicationDbContext>();
    
    protected IModerationMessageService ModerationMessageService =>
        _serviceProvider.GetRequiredService<IModerationMessageService>();

    /// <summary>
    ///     Sets up the services for the integration tests.
    /// </summary>
    [SetUp]
    public void SetUpServices()
    {
        var services = new ServiceCollection();
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            options.Lifetime = ServiceLifetime.Singleton;
        });
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDb");
            options.EnableSensitiveDataLogging();
        }, ServiceLifetime.Singleton);
        services.AddSingleton(new DiscordConfiguration
        {
            DefaultPrefix = "!",
            MaxPrefixLength = 5
        });
        services.AddSingleton(Substitute.For<DiscordSocketClient>());
        services.AddSingleton(Substitute.For<ILoggingService>());
        services.AddSingleton(Substitute.For<IModerationMessageService>());
        services.AddSingleton(Substitute.For<IDiscordClient>());
        services.AddSingleton(GetUnbanSchedulingService());
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    [TearDown]
    public void TearDown()
    {
        TestDbContext.Database.EnsureDeleted();
    }

    /// <summary>
    ///     Send a request to the mediator and return the response.
    /// </summary>
    /// <param name="request"></param>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <returns></returns>
    protected async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    ///     Send a request to the mediator.
    /// </summary>
    /// <param name="request"></param>
    protected async Task SendAsync(IRequest request)
    {
        await _mediator.Send(request);
    }

    /// <summary>
    ///     Add an entity to the test database.
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected void AddEntity<TEntity>(TEntity entity) where TEntity : class
    {
        TestDbContext.Add(entity);
        TestDbContext.SaveChanges();
    }

    /// <summary>
    ///     Set up a mock for the unban scheduling service.
    /// </summary>
    /// <returns></returns>
    private IUnbanSchedulingService GetUnbanSchedulingService()
    {
        var substitute = Substitute.For<IUnbanSchedulingService>();

        // Make ScheduleBanExpiration return a sample GUID
        substitute.ScheduleBanExpiration(Arg.Any<Moderation>()).Returns(Guid.Empty.ToString());

        return substitute;
    }
}