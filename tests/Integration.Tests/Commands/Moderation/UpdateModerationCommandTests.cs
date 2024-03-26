using Application.Commands.Moderation;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation;

public class UpdateModerationCommandTests : TestBase
{
    private IGuild _guild = null!;
    private IGuildUser _user = null!;

    [SetUp]
    public void SetUp()
    {
        _user = DiscordTestUtils.CreateGuildUser(1);
        _guild = DiscordTestUtils.CreateGuild(10);
        DiscordTestUtils.LinkGuildUser(_guild, _user);
        DiscordTestUtils.LinkGuild(DiscordClient, _guild);
    }

    [Test]
    public async Task ShouldUpdateModeration()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.Reason = "updated";
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1)?.Reason.Should().Be("updated");
    }

    [Test]
    public async Task ShouldRescheduleTimedUnbans()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        AddEntity(moderation);
        moderation.ExpiresAt = DateTime.UtcNow.AddHours(2);
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        UnbanSchedulingService.Received().UpdateBanExpiration(moderation);
    }

    [Test]
    public async Task ShouldCancelScheduledUnbans()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        AddEntity(moderation);
        moderation.ExpiresAt = null;
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        UnbanSchedulingService.Received().CancelBanExpiration(moderation);
        moderation.JobId.Should().BeNull();
    }

    [Test]
    public async Task ShouldScheduleUnbans()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.ExpiresAt = DateTime.UtcNow.AddHours(1);
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        UnbanSchedulingService.Received().ScheduleBanExpiration(moderation);
    }

    [Test]
    public async Task ShouldNotCallUnbanSchedulerForNonBans()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Mute,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.ExpiresAt = DateTime.UtcNow.AddHours(1);
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        UnbanSchedulingService.DidNotReceive().ScheduleBanExpiration(moderation);
    }

    [Test]
    public async Task ShouldThrowIfModerationDoesNotExist()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test"
        };
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldThrowIfModerationTypeIsChanged()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.Type = ModerationType.Mute;
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldThrowIfJobIdIsChanged()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.JobId = "test";
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldAllowChangingJobIdWhenForced()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Warning,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.JobId = "test";
        var request = new UpdateModerationCommand
        {
            Moderation = moderation,
            ForceAllowJobIdChange = true
        };
        await SendAsync(request);
        moderation.JobId.Should().Be("test");
    }

    [Test]
    public async Task ShouldNotUpdateExpirationTimeForInactiveModerations()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        AddEntity(moderation);
        moderation.ExpiresAt = DateTime.UtcNow.AddHours(2);
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        moderation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(-1), TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task ShouldUpdateTimeoutDurationForMutes()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Mute,
            Reason = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        AddEntity(moderation);
        moderation.ExpiresAt = DateTime.UtcNow.AddHours(2);
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        await DiscordClient.Received().GetGuildAsync(_guild.Id);
        await _guild.Received().GetUserAsync(_user.Id);
        await _user.Received().SetTimeOutAsync(Arg.Any<TimeSpan>(), Arg.Any<RequestOptions>());
    }

    [Test]
    public async Task ShouldSetTimeoutToMaxValueWhenExpiresAtIsNull()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Mute,
            Reason = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        AddEntity(moderation);
        moderation.ExpiresAt = null;
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        await DiscordClient.Received().GetGuildAsync(_guild.Id);
        await _guild.Received().GetUserAsync(_user.Id);
        await _user.Received().SetTimeOutAsync(TimeSpan.MaxValue, Arg.Any<RequestOptions>());
    }

    [Test]
    public async Task ShouldThrowIfExpirationTimeIsInThePast()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        AddEntity(moderation);
        moderation.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task ShouldNotValidateExpirationTimeWhenNotChanged()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test",
            // Set to past time to ensure a validation rule would be triggered if checked
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        AddEntity(moderation);
        moderation.Reason = "updated";
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await SendAsync(request);
        TestDbContext.Moderations.FirstOrDefault(x => x.UserId == 1)?.Reason.Should().Be("updated");
    }
    
    [Test]
    public async Task ShouldThrowIfGuildIdIsChanged()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Type = ModerationType.Ban,
            Reason = "test"
        };
        AddEntity(moderation);
        moderation.GuildId = 100;
        var request = new UpdateModerationCommand
        {
            Moderation = moderation
        };
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>();
    }
}