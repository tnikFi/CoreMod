using Application.Commands.Moderation;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation;

public class MuteUserCommandTests : TestBase
{
    [Test]
    public async Task ShouldAddModeration()
    {
        var user = Substitute.For<IGuildUser>();
        var moderator = Substitute.For<IGuildUser>();
        var guild = Substitute.For<IGuild>();
        user.Id.Returns(1ul);
        moderator.Id.Returns(2ul);
        guild.Id.Returns(10ul);
        var request = new MuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test"
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1)?.Reason.Should().Be("test");
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1)?.Type.Should().Be(ModerationType.Mute);
    }
    
    [Test]
    public async Task ShouldTimeOutUser()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new MuteUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Duration = TimeSpan.FromDays(7)
        };
        await SendAsync(request);
        await user.Received().SetTimeOutAsync(TimeSpan.FromDays(7), Arg.Any<RequestOptions>());
    }
}