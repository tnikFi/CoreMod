using Application.Queries.Configuration;
using Domain.Models;
using FluentAssertions;

namespace Integration.Tests.Queries.Configuration;

[TestFixture]
public class GetCommandPrefixQueryTests : TestBase
{
    [Test]
    public async Task ShouldGetCommandPrefixIfExists()
    {
        AddEntity(new GuildSettings { GuildId = 1, CommandPrefix = "$" });
        var result = await SendAsync(new GetCommandPrefixQuery { GuildId = 1 });
        result.Should().Be("$");
    }

    [Test]
    public async Task ShouldReturnDefaultPrefixIfNull()
    {
        var result = await SendAsync(new GetCommandPrefixQuery { GuildId = 1 });
        result.Should().Be("!");
    }
}