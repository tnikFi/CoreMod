using Application.Queries.DiscordApiCalls.SendWelcomeMessage;
using Discord;
using Domain.Models;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Integration.Tests.Queries.DiscordApiCalls.SendWelcomeMessage;

public class SendWelcomeMessageQueryTests : TestBase
{
    private List<string> _messages = null!;
    private IGuildUser _user = null!;

    [SetUp]
    public void SetUp()
    {
        _messages = new List<string>();
        _user = DiscordTestUtils.CreateGuildUser(1);
        _user.GuildId.Returns(10ul);
        _user.Guild.Name.Returns("Guild");

        // Configure the DM channel to send messages to the _messages list.
        // This is done because the IUser.SendMessageAsync method is an extension method and cannot be mocked.
        // Internally it calls the IDMChannel.SendMessageAsync method, so we mock that instead.
        var dmChannel = Substitute.For<IDMChannel>();
        dmChannel.SendMessageAsync(Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<Embed>(),
                Arg.Any<RequestOptions>(),
                Arg.Any<AllowedMentions>(),
                Arg.Any<MessageReference>(),
                Arg.Any<MessageComponent>(),
                Arg.Any<ISticker[]>(),
                Arg.Any<Embed[]>(),
                Arg.Any<MessageFlags>())
            .ReturnsNullForAnyArgs()
            .AndDoes(callInfo =>
            {
                var message = callInfo.Arg<string>();
                _messages.Add(message);
            });

        _user.CreateDMChannelAsync().Returns(dmChannel);
    }

    [Test]
    public async Task ShouldNotSendWelcomeMessageIfWelcomeMessageIsNotSet()
    {
        await SendAsync(new SendWelcomeMessageQuery {User = _user});
        // Check that _user.SendMessageAsync was not called
        _messages.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldSendWelcomeMessageIfWelcomeMessageIsSet()
    {
        AddEntity(new GuildSettings
        {
            GuildId = 10ul,
            WelcomeMessage = "Welcome"
        });

        await SendAsync(new SendWelcomeMessageQuery {User = _user});
        _messages.Should().HaveCount(1);
        _messages[0].Should().Be("Welcome");
    }

    [Test]
    public async Task ShouldFormatWelcomeMessage()
    {
        AddEntity(new GuildSettings
        {
            GuildId = 10ul,
            WelcomeMessage = "Welcome {user} to {guild}"
        });

        await SendAsync(new SendWelcomeMessageQuery {User = _user});
        _messages.Should().HaveCount(1);
        _messages[0].Should().Be("Welcome <@1> to Guild");
    }
}