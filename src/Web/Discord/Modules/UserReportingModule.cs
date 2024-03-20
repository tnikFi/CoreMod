using Application.Queries.Configuration;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using MediatR;
using ContextType = Discord.Interactions.ContextType;

namespace Web.Discord.Modules;

[Name("User Reporting")]
[global::Discord.Interactions.RequireContext(ContextType.Guild)]
public class UserReportingModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public UserReportingModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [MessageCommand("Report")]
    public async Task ReportMessageAsync(IMessage message)
    {
        await DeferAsync(true);
        if (message.Author.IsBot)
        {
            await ModifyOriginalResponseAsync(m => m.Content = "You cannot report bot messages.");
            return;
        }

        var reportChannelId = await _mediator.Send(new GetReportChannelIdQuery
        {
            GuildId = Context.Guild.Id
        });
        if (!reportChannelId.HasValue)
        {
            await ModifyOriginalResponseAsync(m => m.Content = "Reporting is not enabled on this server.");
            return;
        }

        var reportChannel = Context.Guild.GetTextChannel(reportChannelId.Value);
        if (reportChannel is null)
        {
            await ModifyOriginalResponseAsync(m => m.Content = "Reporting is not enabled on this server.");
            return;
        }
        
        var embed = new EmbedBuilder()
            .WithTitle("Message reported by user")
            .WithAuthor(message.Author)
            .WithDescription(message.Content)
            .WithFooter($"Reported by {Context.User}")
            .WithTimestamp(DateTimeOffset.UtcNow)
            .AddField("Jump to message", $"[Click here]({message.GetJumpUrl()})")
            .AddField("Channel", MentionUtils.MentionChannel(message.Channel.Id))
            .AddField("Message ID", message.Id)
            .Build();
        await reportChannel.SendMessageAsync(embed: embed);
        
        await ModifyOriginalResponseAsync(m => m.Content = "Message reported.");
    }
}