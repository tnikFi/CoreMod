using Application.Commands.Moderation.ModerateUser;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation.ModerateUser;

public class ModerateUserCommandTests : TestBase
{
    [Test]
    public async Task ShouldAddModerationWhenReasonIsNotNull()
    {
        var user = Substitute.For<IGuildUser>();
        var moderator = Substitute.For<IGuildUser>();
        var guild = Substitute.For<IGuild>();
        user.Id.Returns(1ul);
        moderator.Id.Returns(2ul);
        guild.Id.Returns(10ul);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Ban
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1ul)?.Reason.Should().Be("test");
    }

    [Test]
    public async Task ShouldAddModerationWhenReasonIsNull()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = null,
            Type = ModerationType.Ban
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1ul)?.Reason.Should().BeNull();
    }

    [Test]
    public async Task ReasonShouldBeNullIfEmptyOrWhitespace()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request1 = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "",
            Type = ModerationType.Ban
        };
        var request2 = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = " \n\t",
            Type = ModerationType.Kick
        };
        var case1 = await SendAsync(request1);
        var case2 = await SendAsync(request2);
        TestDbContext.Moderations.FirstOrDefault(x => x.Id == case1.Id)?.Reason.Should().BeNull();
        TestDbContext.Moderations.FirstOrDefault(x => x.Id == case2.Id)?.Reason.Should().BeNull();
    }
}