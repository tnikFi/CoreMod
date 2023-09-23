using Application.Queries.Configuration.GetLogChannelId;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Queries.GetLogChannelId;

[TestFixture]
public class GetLogChannelIdQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnNullIfGuildSettingsDoesNotExist()
    {
        var request = new GetLogChannelIdQuery {GuildId = 1};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
    
    [Test]
    public async Task ShouldReturnLogChannel()
    {
        AddEntity(new GuildSettings { GuildId = 2, LogChannelId = 20 });
        var request = new GetLogChannelIdQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().Be(20);
    }
    
    [Test]
    public async Task ShouldReturnNullIfLogChannelIdIsNull()
    {
        AddEntity(new GuildSettings { GuildId = 2, LogChannelId = null });
        var request = new GetLogChannelIdQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
}