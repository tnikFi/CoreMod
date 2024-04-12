using Application.Commands.Configuration;
using Discord;
using Domain.Models;
using FluentAssertions;
using NSubstitute;

namespace Integration.Tests.Commands.Configuration;

[TestFixture]
public class SetPublicRolesCommandTests : TestBase
{
    [Test]
    public async Task ShouldSetPublicRoles()
    {
        var request = new SetPublicRolesCommand {GuildId = 1, RoleIds = new ulong[] {1, 2, 3}};
        await SendAsync(request);
        var publicRoles = TestDbContext.PublicRoles.Where(x => x.GuildId == 1).Select(x => x.RoleId).ToList();
        publicRoles.Should().BeEquivalentTo(new ulong[] {1, 2, 3});
    }

    [Test]
    public async Task ShouldReplaceExistingPublicRoles()
    {
        AddEntity(new PublicRole {GuildId = 1, RoleId = 1});
        AddEntity(new PublicRole {GuildId = 1, RoleId = 2});
        var request = new SetPublicRolesCommand {GuildId = 1, RoleIds = new ulong[] {3, 4, 5}};
        await SendAsync(request);
        var publicRoles = TestDbContext.PublicRoles.Where(x => x.GuildId == 1).Select(x => x.RoleId).ToList();
        publicRoles.Should().BeEquivalentTo(new ulong[] {3, 4, 5});
    }

    [Test]
    public async Task ShouldRemoveExistingPublicRolesIfRoleIdsIsNull()
    {
        AddEntity(new PublicRole {GuildId = 1, RoleId = 1});
        AddEntity(new PublicRole {GuildId = 1, RoleId = 2});
        var request = new SetPublicRolesCommand {GuildId = 1};
        await SendAsync(request);
        var publicRoles = TestDbContext.PublicRoles.Where(x => x.GuildId == 1).Select(x => x.RoleId).ToList();
        publicRoles.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldNotAllowMoreThan25PublicRoles()
    {
        var request = new SetPublicRolesCommand {GuildId = 1, RoleIds = Enumerable.Range(1, 26).Select(x => (ulong) x)};
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot have more than 25 public roles.");
    }

    [Test]
    public async Task ShouldThrowIfAnyRoleDoesNotExist()
    {
        AddEntity(new PublicRole {GuildId = 1, RoleId = 1});
        var request = new SetPublicRolesCommand {GuildId = 1, RoleIds = new ulong[] {1, 2}};
        var guild = DiscordTestUtils.CreateGuild(1);
        DiscordTestUtils.LinkGuild(DiscordClient, guild);
        guild.GetRole(2).Returns((IRole) null!);
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("One or more roles are invalid.");
    }
    
    [Test]
    public async Task ShouldThrowIfAnyRoleIsManaged()
    {
        AddEntity(new PublicRole {GuildId = 1, RoleId = 1});
        var request = new SetPublicRolesCommand {GuildId = 1, RoleIds = new ulong[] {1, 2}};
        var guild = DiscordTestUtils.CreateGuild(1);
        DiscordTestUtils.LinkGuild(DiscordClient, guild);
        var role = Substitute.For<IRole>();
        role.IsManaged.Returns(true);
        guild.GetRole(2).Returns(role);
        await FluentActions.Awaiting(() => SendAsync(request)).Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("One or more roles are invalid.");
    }
}