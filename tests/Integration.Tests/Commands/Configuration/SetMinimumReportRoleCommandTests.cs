using Application.Commands.Configuration;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Commands.Configuration;

public class SetMinimumReportRoleCommandTests : TestBase
{
    [Test]
    public async Task ShouldCreateGuildSettingsIfDoesNotExist()
    {
        var request = new SetMinimumReportRoleCommand {GuildId = 1, RoleId = 1};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.MinimumReportRole.Should().Be(1);
    }

    [Test]
    public async Task ShouldUpdateReportRoleIdIfNotNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, MinimumReportRole = 20});
        var request = new SetMinimumReportRoleCommand {GuildId = 2, RoleId = 30};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.MinimumReportRole.Should().Be(30);
    }

    [Test]
    public async Task ShouldSetReportRoleToNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, MinimumReportRole = 20});
        var request = new SetMinimumReportRoleCommand {GuildId = 2, RoleId = null};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.MinimumReportRole.Should().BeNull();
    }

    [Test]
    public async Task ShouldUpdateReportRoleIdIfNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, MinimumReportRole = null});
        var request = new SetMinimumReportRoleCommand {GuildId = 2, RoleId = 30};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.MinimumReportRole.Should().Be(30);
    }
}