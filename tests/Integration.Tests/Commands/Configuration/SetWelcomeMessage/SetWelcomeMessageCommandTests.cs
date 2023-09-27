using Application.Commands.Configuration.SetWelcomeMessage;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Commands.Configuration.SetWelcomeMessage;

[TestFixture]
public class SetWelcomeMessageTests : TestBase
{
    [Test]
    public async Task ShouldCreateGuildSettingsIfDoesNotExist()
    {
        var request = new SetWelcomeMessageCommand {GuildId = 1, WelcomeMessage = "Welcome"};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.WelcomeMessage.Should().Be("Welcome");
    }

    [Test]
    public async Task ShouldUpdateWelcomeMessageIfNotNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, WelcomeMessage = "Old message"});
        var request = new SetWelcomeMessageCommand {GuildId = 2, WelcomeMessage = "Welcome"};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.WelcomeMessage.Should().Be("Welcome");
    }

    [Test]
    public async Task ShouldSetWelcomeMessageToNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, WelcomeMessage = "Welcome"});
        var request = new SetWelcomeMessageCommand {GuildId = 2, WelcomeMessage = null};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.WelcomeMessage.Should().BeNull();
    }

    [Test]
    public async Task ShouldUpdateWelcomeMessageIfNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, WelcomeMessage = null});
        var request = new SetWelcomeMessageCommand {GuildId = 2, WelcomeMessage = "Welcome"};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.WelcomeMessage.Should().Be("Welcome");
    }
}