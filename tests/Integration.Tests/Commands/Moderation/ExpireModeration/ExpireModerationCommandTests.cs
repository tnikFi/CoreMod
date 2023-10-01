using Application.Commands.Moderation.PardonModeration;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Integration.Tests.Commands.Moderation.ExpireModeration;

[TestFixture]
public class ExpireModerationCommandTests : TestBase
{
    [Test]
    public async Task ShouldDeactivateModeration()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var moderation = new Domain.Models.Moderation
        {
            GuildId = guild.Id,
            UserId = user.Id,
            ModeratorId = moderator.Id,
            Type = ModerationType.Ban,
            Timestamp = DateTime.UtcNow
        };
        AddEntity(moderation);
        var request = new PardonModerationCommand
        {
            Moderation = moderation,
            Moderator = moderator
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1ul && x.Type == ModerationType.Ban)?.Active
            .Should().BeFalse();
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1ul && x.Type == ModerationType.Unban)?.IsPardon
            .Should().BeTrue();
    }

    [Test]
    public async Task ShouldLinkRelatedCases()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var moderation = new Domain.Models.Moderation
        {
            GuildId = guild.Id,
            UserId = user.Id,
            ModeratorId = moderator.Id,
            Type = ModerationType.Ban,
            Timestamp = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        AddEntity(moderation);
        var request = new PardonModerationCommand
        {
            Moderation = moderation,
            Moderator = moderator
        };
        var result = await SendAsync(request);
        TestDbContext.Moderations.Include(x => x.RelatedCase)
            .FirstOrDefault(x => x.UserId == 1ul && x.Type == ModerationType.Ban)?.RelatedCase
            .Should().Be(result);
        TestDbContext.Moderations.Include(x => x.RelatedCase)
            .FirstOrDefault(x => x.UserId == 1ul && x.Type == ModerationType.Unban)?.RelatedCase
            .Should().Be(moderation);
    }

    [Test]
    public async Task ShouldThrowIfModerationIsExpired()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var moderation = new Domain.Models.Moderation
        {
            GuildId = guild.Id,
            UserId = user.Id,
            ModeratorId = moderator.Id,
            Type = ModerationType.Ban,
            Timestamp = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        AddEntity(moderation);
        var request = new PardonModerationCommand
        {
            Moderation = moderation,
            Moderator = moderator
        };
        await FluentActions.Invoking(async () => await SendAsync(request)).Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldThrowIfModerationIsPardoned()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var moderation = new Domain.Models.Moderation
        {
            GuildId = guild.Id,
            UserId = user.Id,
            ModeratorId = moderator.Id,
            Type = ModerationType.Ban,
            Timestamp = DateTime.UtcNow
        };
        AddEntity(moderation);
        var request = new PardonModerationCommand
        {
            Moderation = moderation,
            Moderator = moderator
        };
        await SendAsync(request);
        await FluentActions.Invoking(async () => await SendAsync(request)).Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldThrowIfModerationIsNotPardonable()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var moderation = new Domain.Models.Moderation
        {
            GuildId = guild.Id,
            UserId = user.Id,
            ModeratorId = moderator.Id,
            Type = ModerationType.Warning,
            Timestamp = DateTime.UtcNow
        };
        AddEntity(moderation);
        var request = new PardonModerationCommand
        {
            Moderation = moderation,
            Moderator = moderator
        };
        await FluentActions.Invoking(async () => await SendAsync(request)).Should()
            .ThrowAsync<InvalidOperationException>("The moderation type can not be pardoned.");
    }
}