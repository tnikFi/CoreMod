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
        user.Mention.Returns($"<@{id}>");
        return user;
    }

    public static void LinkGuildUser(IGuild guild, IGuildUser user)
    {
        user.Guild.Returns(guild);
        guild.GetUserAsync(user.Id, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(user);
    }

    public static void LinkGuild(IDiscordClient client, IGuild guild)
    {
        client.GetGuildAsync(guild.Id, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(guild);
    }

    public static IGuild CreateGuild(ulong id, string? name = null)
    {
        var guild = Substitute.For<IGuild>();
        guild.Id.Returns(id);
        guild.Name.Returns(name);
        return guild;
    }
}