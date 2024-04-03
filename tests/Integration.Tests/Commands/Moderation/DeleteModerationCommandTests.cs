using Application.Commands.Moderation;
using Discord;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Moderation;

public class DeleteModerationCommandTests : TestBase
{
    private IGuild _guild = null!;
    private IGuildUser _user = null!;
    private IGuildUser _deletedBy = null!;

    [SetUp]
    public void SetUp()
    {
        _guild = DiscordTestUtils.CreateGuild(10);
        _user = DiscordTestUtils.CreateGuildUser(_guild, 1);
        _deletedBy = DiscordTestUtils.CreateGuildUser(_guild, 2);
        DiscordTestUtils.LinkGuild(DiscordClient, _guild);
    }
    
    [Test]
    public async Task ShouldDeleteModeration()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            Id = 1
        };
        AddEntity(moderation);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id
        };
        await SendAsync(request);
        TestDbContext.Moderations.Should().BeEmpty();
    }
    
    [Test]
    public async Task ShouldNotDeleteModerationIfCaseDoesNotExist()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            Id = 1
        };
        AddEntity(moderation);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = 2
        };
        await SendAsync(request);
        TestDbContext.Moderations.Should().Contain(moderation);
    }
    
    [Test]
    public async Task ShouldNotDeleteModerationIfGuildIdDoesNotMatch()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            Id = 1
        };
        AddEntity(moderation);
        var request = new DeleteModerationCommand
        {
            GuildId = 11,
            CaseId = moderation.Id
        };
        await SendAsync(request);
        TestDbContext.Moderations.Should().Contain(moderation);
    }
    
    [Test]
    public async Task ShouldRemoveTimeoutIfPardonModerationIsTrue()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Mute,
            Id = 1
        };
        AddEntity(moderation);
        _user.TimedOutUntil.Returns(DateTimeOffset.UtcNow.AddMinutes(1));
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id,
            DeletedBy = _deletedBy,
            PardonModeration = true
        };
        await SendAsync(request);
        await _user.Received().RemoveTimeOutAsync(Arg.Any<RequestOptions>());
        await ModerationMessageService.Received().SendModerationDeletedMessageAsync(moderation, _deletedBy);
    }

    [Test]
    public async Task ShouldNotRemoveTimeoutIfNoTimeoutActive()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Mute,
            Id = 1
        };
        AddEntity(moderation);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id,
            DeletedBy = _deletedBy,
            PardonModeration = true
        };
        await SendAsync(request);
        await _user.DidNotReceiveWithAnyArgs().RemoveTimeOutAsync(Arg.Any<RequestOptions>());
        await ModerationMessageService.Received().SendModerationDeletedMessageAsync(moderation, _deletedBy, false);
    }
    
    [Test]
    public async Task ShouldUnbanIfPardonModerationIsTrue()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            Id = 1
        };
        AddEntity(moderation);
        var ban = Substitute.For<IBan>();
        _guild.GetBanAsync(_user.Id).Returns(ban);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id,
            DeletedBy = _deletedBy,
            PardonModeration = true
        };
        await SendAsync(request);
        await _guild.Received().RemoveBanAsync(_user.Id, Arg.Any<RequestOptions>());
        await ModerationMessageService.Received().SendModerationDeletedMessageAsync(moderation, _deletedBy);
    }

    [Test]
    public async Task ShouldCancelUnbanIfBanIsRemoved()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
            Id = 1
        };
        AddEntity(moderation);
        var ban = Substitute.For<IBan>();
        _guild.GetBanAsync(_user.Id).Returns(ban);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id,
            DeletedBy = _deletedBy,
            PardonModeration = true
        };
        await SendAsync(request);
        await _guild.Received().RemoveBanAsync(_user.Id, Arg.Any<RequestOptions>());
        await ModerationMessageService.Received().SendModerationDeletedMessageAsync(moderation, _deletedBy);
        UnbanSchedulingService.Received().CancelBanExpiration(moderation);
    }

    [Test]
    public async Task ShouldNotCancelUnbanForExpiredBan()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            Id = 1
        };
        AddEntity(moderation);
        var ban = Substitute.For<IBan>();
        _guild.GetBanAsync(_user.Id).Returns(ban);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id,
            DeletedBy = _deletedBy,
            PardonModeration = true
        };
        await SendAsync(request);
        await _guild.Received().RemoveBanAsync(_user.Id, Arg.Any<RequestOptions>());
        await ModerationMessageService.Received().SendModerationDeletedMessageAsync(moderation, _deletedBy);
        UnbanSchedulingService.DidNotReceiveWithAnyArgs().CancelBanExpiration(Arg.Any<Domain.Models.Moderation>());
    }
    
    [Test]
    public async Task ShouldNotSendDMForUnbanIfNoBanExists()
    {
        var moderation = new Domain.Models.Moderation
        {
            GuildId = _guild.Id,
            UserId = _user.Id,
            Reason = "test",
            Type = ModerationType.Ban,
            Id = 1
        };
        AddEntity(moderation);
        _guild.GetBanAsync(_user.Id).Returns((IBan?) null);
        var request = new DeleteModerationCommand
        {
            GuildId = _guild.Id,
            CaseId = moderation.Id,
            DeletedBy = _deletedBy,
            PardonModeration = true
        };
        await SendAsync(request);
        await ModerationMessageService.Received().SendModerationDeletedMessageAsync(moderation, _deletedBy, false);
        UnbanSchedulingService.DidNotReceiveWithAnyArgs().CancelBanExpiration(Arg.Any<Domain.Models.Moderation>());
    }
}