using Application.Commands.Configuration.SetCommandPrefix;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Commands.SetCommandPrefix;

[TestFixture]
public class SetCommandPrefixCommandTests : TestBase
{
    [Test]
    public async Task ShouldSetPrefixIfGuildSettingsDoNotExist()
    {
        await SendAsync(new SetCommandPrefixCommand {GuildId = 1, Prefix = "$"});
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.CommandPrefix.Should().Be("$");
    }

    [Test]
    public async Task ShouldUpdatePrefixIfGuildSettingsExist()
    {
        AddEntity(new GuildSettings { GuildId = 1 });
        await SendAsync(new SetCommandPrefixCommand {GuildId = 1, Prefix = "$"});
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.CommandPrefix.Should().Be("$");
    }

    [Test]
    public async Task ShouldSetPrefixToDefaultIfNull()
    {
        AddEntity(new GuildSettings { GuildId = 1, CommandPrefix = "$" });
        await SendAsync(new SetCommandPrefixCommand {GuildId = 1, Prefix = null});
        TestDbContext.GuildSettings.FirstOrDefault(x => x.GuildId == 1)?.CommandPrefix.Should().Be("!");
    }
}