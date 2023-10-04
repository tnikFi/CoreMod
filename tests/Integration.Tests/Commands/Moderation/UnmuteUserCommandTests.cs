using Application.Commands.Moderation;
using Discord;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation;

public class UnmuteUserCommandTests : TestBase
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
        var request = new UnmuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test"
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
        var request = new UnmuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = null
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
        var request1 = new UnmuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = ""
        };
        var request2 = new UnmuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = " \n\t"
        };
        var case1 = await SendAsync(request1);
        var case2 = await SendAsync(request2);
        TestDbContext.Moderations.FirstOrDefault(x => x.Id == case1.Id)?.Reason.Should().BeNull();
        TestDbContext.Moderations.FirstOrDefault(x => x.Id == case2.Id)?.Reason.Should().BeNull();
    }

    [Test]
    public async Task ShouldRemoveUserTimeOut()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new UnmuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test"
        };
        await SendAsync(request);
        await user.Received().RemoveTimeOutAsync(Arg.Any<RequestOptions>());
    }
}