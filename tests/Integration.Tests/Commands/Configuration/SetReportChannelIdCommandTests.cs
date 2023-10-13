using Application.Commands.Configuration;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Commands.Configuration;

[TestFixture]
public class SetReportChannelIdCommandTests : TestBase
{
    [Test]
    public async Task ShouldCreateGuildSettingsIfDoesNotExist()
    {
        var request = new SetReportChannelIdCommand {GuildId = 1, ReportChannelId = 1};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.ReportChannelId.Should().Be(1);
    }

    [Test]
    public async Task ShouldUpdateReportChannelIdIfNotNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, ReportChannelId = 20});
        var request = new SetReportChannelIdCommand {GuildId = 2, ReportChannelId = 30};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.ReportChannelId.Should().Be(30);
    }

    [Test]
    public async Task ShouldSetReportChannelIdToNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, ReportChannelId = 20});
        var request = new SetReportChannelIdCommand {GuildId = 2, ReportChannelId = null};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.ReportChannelId.Should().BeNull();
    }

    [Test]
    public async Task ShouldUpdateReportChannelIdIfNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, ReportChannelId = null});
        var request = new SetReportChannelIdCommand {GuildId = 2, ReportChannelId = 30};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.ReportChannelId.Should().Be(30);
    }
}