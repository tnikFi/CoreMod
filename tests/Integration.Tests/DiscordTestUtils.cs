using Discord;
using NSubstitute;

namespace Integration.Tests;

public static class DiscordTestUtils
{
    public static IGuildUser CreateGuildUser(ulong id, string? name = null)
    {
        var user = Substitute.For<IGuildUser>();
        user.Id.Returns(id);
        user.Username.Returns(name);
        return user;
    }
    
    public static IGuild CreateGuild(ulong id, string? name = null)
    {
        var guild = Substitute.For<IGuild>();
        guild.Id.Returns(id);
        guild.Name.Returns(name);
        return guild;
    }
}