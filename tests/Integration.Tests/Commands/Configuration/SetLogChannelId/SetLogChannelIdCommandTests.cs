using Application.Commands.Configuration.SetLogChannelId;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Commands.Configuration.SetLogChannelId;

[TestFixture]
public class SetLogChannelIdCommandTests : TestBase
{
    [Test]
    public async Task ShouldCreateGuildSettingsIfDoesNotExist()
    {
        var request = new SetLogChannelIdCommand {GuildId = 1, LogChannelId = 1};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.LogChannelId.Should().Be(1);
    }

    [Test]
    public async Task ShouldUpdateLogChannelIdIfNotNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, LogChannelId = 20});
        var request = new SetLogChannelIdCommand {GuildId = 2, LogChannelId = 30};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.LogChannelId.Should().Be(30);
    }

    [Test]
    public async Task ShouldSetLogChannelIdToNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, LogChannelId = 20});
        var request = new SetLogChannelIdCommand {GuildId = 2, LogChannelId = null};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.LogChannelId.Should().BeNull();
    }

    [Test]
    public async Task ShouldUpdateLogChannelIdIfNull()
    {
        AddEntity(new GuildSettings {GuildId = 2, LogChannelId = null});
        var request = new SetLogChannelIdCommand {GuildId = 2, LogChannelId = 30};
        await SendAsync(request);
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 2)?.LogChannelId.Should().Be(30);
    }
}