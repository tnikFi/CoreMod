using Application.Queries.Configuration;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Queries.Configuration;

[TestFixture]
public class GetPublicRolesQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyListIfNoPublicRolesExist()
    {
        var request = new GetPublicRolesQuery {GuildId = 1};
        var result = await SendAsync(request);
        result.Should().BeEmpty();
    }
    
    [Test]
    public async Task ShouldReturnPublicRoles()
    {
        AddEntity(new PublicRole { GuildId = 2, RoleId = 20 });
        AddEntity(new PublicRole { GuildId = 2, RoleId = 21 });
        var request = new GetPublicRolesQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().BeEquivalentTo(new ulong[] {20, 21});
    }
    
    [Test]
    public async Task ShouldReturnEmptyListIfNoPublicRolesExistForGuild()
    {
        AddEntity(new PublicRole { GuildId = 2, RoleId = 20 });
        AddEntity(new PublicRole { GuildId = 2, RoleId = 21 });
        var request = new GetPublicRolesQuery {GuildId = 3};
        var result = await SendAsync(request);
        result.Should().BeEmpty();
    }
    
    [Test]
    public async Task ShouldReturnRolesForCorrectGuild()
    {
        AddEntity(new PublicRole { GuildId = 2, RoleId = 20 });
        AddEntity(new PublicRole { GuildId = 2, RoleId = 21 });
        AddEntity(new PublicRole { GuildId = 3, RoleId = 22 });
        var request = new GetPublicRolesQuery {GuildId = 2};
        var result = await SendAsync(request);
        result.Should().BeEquivalentTo(new ulong[] {20, 21});
    }
}