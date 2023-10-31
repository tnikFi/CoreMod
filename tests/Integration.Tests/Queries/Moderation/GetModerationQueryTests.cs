using Application.Queries.Moderation;
using FluentAssertions;

namespace Integration.Tests.Queries.Moderation;

public class GetModerationQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnNullIfNoModeration()
    {
        var request = new GetModerationQuery
        {
            Id = 1,
            Guild = DiscordTestUtils.CreateGuild(1)
        };
        var result = await SendAsync(request);
        result.Should().BeNull();
    }

    [Test]
    public async Task ShouldReturnModeration()
    {
        var guild = DiscordTestUtils.CreateGuild(1);
        AddEntity(new Domain.Models.Moderation
        {
            GuildId = guild.Id
        });
        var request = new GetModerationQuery
        {
            Id = 1,
            Guild = guild
        };
        var result = await SendAsync(request);
        result.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldNotReturnModerationForIncorrectGuild()
    {
        var guild1 = DiscordTestUtils.CreateGuild(1);
        var guild2 = DiscordTestUtils.CreateGuild(2);
        AddEntity(new Domain.Models.Moderation
        {
            GuildId = guild1.Id
        });
        var request = new GetModerationQuery
        {
            Id = 1,
            Guild = guild2
        };
        var result = await SendAsync(request);
        result.Should().BeNull();
    }
}