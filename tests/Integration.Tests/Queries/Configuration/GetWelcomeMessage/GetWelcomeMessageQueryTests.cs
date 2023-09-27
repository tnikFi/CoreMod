using Application.Queries.Configuration.GetWelcomeMessage;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Queries.Configuration.GetWelcomeMessage;

[TestFixture]
public class GetWelcomeMessageQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnNullIfGuildSettingsDoesNotExist()
    {
        var request = new GetWelcomeMessageQuery {GuildId = 1};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnWelcomeMessage()
    {
        AddEntity(new GuildSettings {GuildId = 2, WelcomeMessage = "Welcome"});
        var request = new GetWelcomeMessageQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().Be("Welcome");
    }

    [Test]
    public async Task ShouldReturnNullIfWelcomeMessageIsNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, WelcomeMessage = null});
        var request = new GetWelcomeMessageQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
}