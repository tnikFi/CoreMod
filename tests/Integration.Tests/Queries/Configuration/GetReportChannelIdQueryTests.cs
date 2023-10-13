using Application.Queries.Configuration;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Queries.Configuration;

[TestFixture]
public class GetReportChannelIdQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnNullIfGuildSettingsDoesNotExist()
    {
        var request = new GetReportChannelIdQuery {GuildId = 1};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
    
    [Test]
    public async Task ShouldReturnReportChannel()
    {
        AddEntity(new GuildSettings { GuildId = 2, ReportChannelId = 20 });
        var request = new GetReportChannelIdQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().Be(20);
    }
    
    [Test]
    public async Task ShouldReturnNullIfReportChannelIdIsNull()
    {
        AddEntity(new GuildSettings { GuildId = 2, ReportChannelId = null });
        var request = new GetReportChannelIdQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
}