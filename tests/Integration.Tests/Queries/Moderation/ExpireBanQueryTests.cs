using Application.Queries.Moderation;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Integration.Tests.Queries.Moderation;

[TestFixture]
public class ExpireBanQueryTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        UnbanCount = 0;
        Guild = DiscordTestUtils.CreateGuild(10ul);
        User = DiscordTestUtils.CreateGuildUser(1);

        DiscordClient.GetUserAsync(User.Id).Returns(User);
        DiscordClient.GetGuildAsync(Guild.Id).Returns(Guild);

        // Configure guild.GetBanAsync to return a ban
        Guild.GetBanAsync(User.Id).ReturnsForAnyArgs(callInfo => Task.FromResult(Substitute.For<IBan>()));

        // Configure guild.RemoveBanAsync to increment UnbanCount
        Guild.RemoveBanAsync(User.Id, Arg.Any<RequestOptions>()).Returns(callInfo => Task.CompletedTask)
            .AndDoes(callInfo => { UnbanCount++; });
    }

    private int UnbanCount { get; set; }
    private IGuildUser User { get; set; } = null!;
    private IGuild Guild { get; set; } = null!;

    [Test]
    public async Task ShouldUnbanUser()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow
        };
        AddEntity(moderation);

        await SendAsync(new ExpireBanQuery
        {
            GuildId = Guild.Id,
            ModerationId = moderation.Id
        });

        UnbanCount.Should().Be(1);
    }

    [Test]
    public async Task ShouldThrowIfGuildNotFound()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow
        };
        AddEntity(moderation);

        DiscordClient.GetGuildAsync(Guild.Id).ReturnsNullForAnyArgs();

        await FluentActions.Invoking(() => SendAsync(new ExpireBanQuery
        {
            GuildId = Guild.Id,
            ModerationId = moderation.Id
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldThrowIfModerationNotFound()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow
        };
        AddEntity(moderation);

        await FluentActions.Invoking(() => SendAsync(new ExpireBanQuery
        {
            GuildId = Guild.Id,
            ModerationId = moderation.Id + 1
        })).Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldDoNothingIfUserNotBanned()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow
        };
        AddEntity(moderation);

        // Configure guild.GetBanAsync to return null (user is not banned)
        Guild.GetBanAsync(User.Id).ReturnsNullForAnyArgs();

        await SendAsync(new ExpireBanQuery
        {
            GuildId = Guild.Id,
            ModerationId = moderation.Id
        });

        UnbanCount.Should().Be(0);
    }

    [Test]
    public async Task ShouldDoNothingIfBanIsNotExpired()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        AddEntity(moderation);

        await SendAsync(new ExpireBanQuery
        {
            GuildId = Guild.Id,
            ModerationId = moderation.Id
        });

        UnbanCount.Should().Be(0);
    }

    [Test]
    public async Task ShouldDoNothingIfUserHasNewerBan()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        AddEntity(moderation);

        var newBan = new Domain.Models.Moderation
        {
            GuildId = Guild.Id,
            UserId = User.Id,
            Type = ModerationType.Ban,
            Timestamp = DateTime.UtcNow
        };
        AddEntity(newBan);

        await SendAsync(new ExpireBanQuery
        {
            GuildId = Guild.Id,
            ModerationId = moderation.Id
        });

        UnbanCount.Should().Be(0);
    }
}