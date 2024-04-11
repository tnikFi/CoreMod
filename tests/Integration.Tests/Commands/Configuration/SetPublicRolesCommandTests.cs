using Application.Commands.Configuration;
using Domain.Models;
using FluentAssertions;

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
}