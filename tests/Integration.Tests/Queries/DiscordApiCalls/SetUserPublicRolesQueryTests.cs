using Application.Queries.DiscordApiCalls;
using Discord;
using Domain.Models;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Queries.DiscordApiCalls;

[TestFixture]
public class SetUserPublicRolesQueryTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        _guild = DiscordTestUtils.CreateGuild(1);
        DiscordTestUtils.LinkGuild(DiscordClient, _guild);
        _user = DiscordTestUtils.CreateGuildUser(_guild, 10);
    }

    private IGuild _guild = null!;
    private IGuildUser _user = null!;

    [Test]
    public async Task ShouldAddPublicRolesToUser()
    {
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = new List<ulong> {1, 2, 3},
            PublicRoleIds = new List<ulong> {1, 2, 3}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {1, 2, 3})));
    }

    [Test]
    public async Task ShouldRemovePublicRolesFromUser()
    {
        _user.RoleIds.Returns(new List<ulong> {4, 5, 6});
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = Array.Empty<ulong>(),
            PublicRoleIds = new List<ulong> {4, 5, 6}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.Received().RemoveRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {4, 5, 6})));
    }

    [Test]
    public async Task ShouldNotAddNonPublicRoles()
    {
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = new List<ulong> {1, 2, 3},
            PublicRoleIds = new List<ulong> {1, 2}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {1, 2})));
    }

    [Test]
    public async Task ShouldNotRemoveNonPublicRoles()
    {
        _user.RoleIds.Returns(new List<ulong> {4, 5, 6});
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = Array.Empty<ulong>(),
            PublicRoleIds = new List<ulong> {4, 5}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.Received().RemoveRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {4, 5})));
    }

    [Test]
    public async Task ShouldNotAddOrRemoveRolesAlreadyAssigned()
    {
        _user.RoleIds.Returns(new List<ulong> {1, 2, 3});
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = new List<ulong> {1, 2, 3},
            PublicRoleIds = new List<ulong> {1, 2, 3}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.DidNotReceive().AddRolesAsync(Arg.Any<IEnumerable<ulong>>());
        await _user.DidNotReceive().RemoveRolesAsync(Arg.Any<IEnumerable<ulong>>());
    }

    [Test]
    public async Task ShouldAddAndRemoveRoles()
    {
        _user.RoleIds.Returns(new List<ulong> {1, 2, 3});
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = new List<ulong> {2, 4},
            PublicRoleIds = new List<ulong> {2, 3, 4}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {4})));
        await _user.Received().RemoveRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {3})));
    }

    [Test]
    public async Task ShouldUsePublicRolesFromDatabaseWhenNotProvided()
    {
        AddEntity(new PublicRole
        {
            GuildId = 1,
            RoleId = 1
        });
        AddEntity(new PublicRole
        {
            GuildId = 1,
            RoleId = 2
        });
        AddEntity(new PublicRole
        {
            GuildId = 1,
            RoleId = 3
        });
        var query = new SetUserPublicRolesQuery
        {
            GuildId = 1,
            UserId = 10,
            RoleIds = new List<ulong> {1, 2, 3}
        };

        await FluentActions.Awaiting(() => SendAsync(query)).Should().NotThrowAsync();
        await _user.Received().AddRolesAsync(Arg.Is<IEnumerable<ulong>>(roles =>
            roles.SequenceEqual(new ulong[] {1, 2, 3})));
    }
}