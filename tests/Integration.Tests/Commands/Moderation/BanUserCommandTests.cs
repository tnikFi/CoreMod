using Application.Commands.Moderation;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation;

public class BanUserCommandTests : TestBase
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
        var request = new BanUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test"
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1)?.Reason.Should().Be("test");
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1)?.Type.Should().Be(ModerationType.Ban);
    }
    
    [Test]
    public async Task ShouldBanUser()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new BanUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test"
        };
        await SendAsync(request);
        await guild.Received().AddBanAsync(user, 0, "test");
    }

    [Test]
    public async Task ShouldAddExpirationTime()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new BanUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
            Duration = TimeSpan.FromDays(7)
        };
        var moderation = await SendAsync(request);
        // Check that the DateTime is within 1 minute of the expected time. The test should never take longer than this to run.
        moderation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(request.Duration.Value), TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task ShouldScheduleBanExpirationForTemporaryBans()
    {
        var user = DiscordTestUtils.CreateGuildUser(1);
        var moderator = DiscordTestUtils.CreateGuildUser(2);
        var guild = DiscordTestUtils.CreateGuild(10);
        var request = new BanUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test",
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
        var request = new BanUserCommand
        {
            Guild = guild,
            User = user,
            Moderator = moderator,
            Reason = "test"
        };
        var result = await SendAsync(request);
        UnbanSchedulingService.DidNotReceive().ScheduleBanExpiration(result);
    }
}