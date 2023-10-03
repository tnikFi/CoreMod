using Application.Queries.Moderation;
using Domain.Enums;
using FluentAssertions;

namespace Integration.Tests.Queries.Moderation;

public class GetModerationsQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyListIfNoModerations()
    {
        var request = new GetModerationsQuery {Guild = DiscordTestUtils.CreateGuild(1)};
        var result = await SendAsync(request);
        result.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldReturnModerationsForCorrectGuild()
    {
        AddEntity(new Domain.Models.Moderation {GuildId = 1});
        AddEntity(new Domain.Models.Moderation {GuildId = 2});
        var request = new GetModerationsQuery {Guild = DiscordTestUtils.CreateGuild(1)};
        var result = await SendAsync(request);
        result.Should().HaveCount(1);
        result.First().GuildId.Should().Be(1);
    }

    [Test]
    public async Task ShouldFilterByTypeIfSpecified()
    {
        AddEntity(new Domain.Models.Moderation {GuildId = 1, Type = ModerationType.Ban});
        AddEntity(new Domain.Models.Moderation {GuildId = 1, Type = ModerationType.Warning});
        var request = new GetModerationsQuery {Guild = DiscordTestUtils.CreateGuild(1), Type = ModerationType.Ban};
        var result = await SendAsync(request);
        result.Should().HaveCount(1);
        result.First().Type.Should().Be(ModerationType.Ban);
    }

    [Test]
    public async Task ShouldFilterByUserIfSpecified()
    {
        var user1 = DiscordTestUtils.CreateGuildUser(1);
        var user2 = DiscordTestUtils.CreateGuildUser(2);
        AddEntity(new Domain.Models.Moderation {GuildId = 1, UserId = user1.Id});
        AddEntity(new Domain.Models.Moderation {GuildId = 1, UserId = user2.Id});
        var request = new GetModerationsQuery {Guild = DiscordTestUtils.CreateGuild(1), User = user1};
        var result = await SendAsync(request);
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(user1.Id);
    }

    [Test]
    public async Task ShouldFilterByModeratorIfSpecified()
    {
        var moderator1 = DiscordTestUtils.CreateGuildUser(1);
        var moderator2 = DiscordTestUtils.CreateGuildUser(2);
        AddEntity(new Domain.Models.Moderation {GuildId = 1, ModeratorId = moderator1.Id});
        AddEntity(new Domain.Models.Moderation {GuildId = 1, ModeratorId = moderator2.Id});
        var request = new GetModerationsQuery {Guild = DiscordTestUtils.CreateGuild(1), Moderator = moderator1};
        var result = await SendAsync(request);
        result.Should().HaveCount(1);
        result.First().ModeratorId.Should().Be(moderator1.Id);
    }

    [Test]
    public async Task ShouldCombineFilterCriteria()
    {
        var user1 = DiscordTestUtils.CreateGuildUser(1);
        var user2 = DiscordTestUtils.CreateGuildUser(2);
        var moderator1 = DiscordTestUtils.CreateGuildUser(3);
        var moderator2 = DiscordTestUtils.CreateGuildUser(4);
        AddEntity(new Domain.Models.Moderation
            {GuildId = 1, Type = ModerationType.Ban, UserId = user1.Id, ModeratorId = moderator1.Id});
        AddEntity(new Domain.Models.Moderation
            {GuildId = 1, Type = ModerationType.Ban, UserId = user1.Id, ModeratorId = moderator2.Id});
        AddEntity(new Domain.Models.Moderation
            {GuildId = 1, Type = ModerationType.Ban, UserId = user2.Id, ModeratorId = moderator1.Id});
        AddEntity(new Domain.Models.Moderation
            {GuildId = 1, Type = ModerationType.Warning, UserId = user1.Id, ModeratorId = moderator1.Id});
        var request = new GetModerationsQuery
            {Guild = DiscordTestUtils.CreateGuild(1), Type = ModerationType.Ban, Moderator = moderator1};
        var result = await SendAsync(request);
        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.Type == ModerationType.Ban && x.ModeratorId == moderator1.Id);
    }
}