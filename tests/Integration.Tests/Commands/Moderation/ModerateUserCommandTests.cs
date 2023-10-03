using Application.Commands.Moderation;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation;

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

    [Test]
    public async Task ShouldTimeOutUser()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Mute,
            Duration = TimeSpan.FromDays(7)
        };
        await SendAsync(request);
        await user.Received().SetTimeOutAsync(TimeSpan.FromDays(7), Arg.Any<RequestOptions>());
    }

    [Test]
    public async Task ShouldRemoveUserTimeOut()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Unmute
        };
        await SendAsync(request);
        await user.Received().RemoveTimeOutAsync(Arg.Any<RequestOptions>());
    }

    [Test]
    public async Task ShouldKickUser()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Kick
        };
        await SendAsync(request);
        await user.Received().KickAsync("test");
    }

    [Test]
    public async Task ShouldBanUser()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Ban
        };
        await SendAsync(request);
        await guild.Received().AddBanAsync(user, 0, "test");
    }

    [Test]
    public async Task ShouldUnbanUser()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Unban
        };
        await SendAsync(request);
        await guild.Received().RemoveBanAsync(user, Arg.Any<RequestOptions>());
    }

    [Test]
    public async Task ShouldAddExpirationTime()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Mute,
            Duration = TimeSpan.FromDays(7)
        };
        var moderation = await SendAsync(request);
        // Check that the DateTime is within 1 minute of the expected time. The test should never take longer than this to run.
        moderation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(request.Duration.Value), TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task ShouldNotAddExpirationTimeIfModerationTypeDoesNotSupportTemporaryModerations()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Warning,
            Duration = TimeSpan.FromDays(7)
        };
        var moderation = await SendAsync(request);
        moderation.ExpiresAt.Should().BeNull();
    }

    [Test]
    public async Task ShouldScheduleBanExpirationForTemporaryBans()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Ban,
            Duration = TimeSpan.FromDays(7)
        };
        var result = await SendAsync(request);
        UnbanSchedulingService.Received().ScheduleBanExpiration(result);
    }

    [Test]
    public async Task ShouldNotScheduleBanExpirationForPermanentBans()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new ModerateUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Type = ModerationType.Ban
        };
        var result = await SendAsync(request);
        UnbanSchedulingService.DidNotReceive().ScheduleBanExpiration(result);
    }
}