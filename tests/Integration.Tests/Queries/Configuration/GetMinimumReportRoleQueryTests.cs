using Application.Queries.Configuration;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Queries.Configuration;

[TestFixture]
public class GetMinimumReportRoleQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnNullIfGuildSettingsDoesNotExist()
    {
        var request = new GetMinimumReportRoleIdQuery {GuildId = 1};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
    
    [Test]
    public async Task ShouldReturnReportRole()
    {
        AddEntity(new GuildSettings { GuildId = 2, MinimumReportRole = 20 });
        var request = new GetMinimumReportRoleIdQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().Be(20);
    }
    
    [Test]
    public async Task ShouldReturnNullIfReportRoleIdIsNull()
    {
        AddEntity(new GuildSettings { GuildId = 2, MinimumReportRole = null });
        var request = new GetMinimumReportRoleIdQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
}