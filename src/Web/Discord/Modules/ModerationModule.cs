using Application.Commands.Moderation.ModerateUser;
using Application.Interfaces;
using Common.Extensions;
using Discord;
using Discord.Commands;
using MediatR;

namespace Web.Discord.Modules;

[Name("Moderation")]
[Summary("Moderation commands")]
[RequireContext(ContextType.Guild)]
public class ModerationModule : ModuleBase<SocketCommandContext>
{
    private readonly IMediator _mediator;
    private readonly ILoggingService _loggingService;

    public ModerationModule(IMediator mediator, ILoggingService loggingService)
    {
        _mediator = mediator;
        _loggingService = loggingService;
    }
    
    [Command("warn")]
    [Summary("Warns a user.")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task WarnAsync(
        [Summary("The user to warn.")]
        IGuildUser user,
        [Summary("The reason for the warning.")]
        [Remainder] string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;
        var dmEmbed = new EmbedBuilder()
            .WithTitle($"You were warned in {Context.Guild.Name}")
            .AddField("Reason", reason ?? "No reason provided.")
            .WithColor(Color.Gold)
            .Build();
        var sentDm = await user.TrySendMessageAsync(embed: dmEmbed);
        
        var auditLogEmbed = new EmbedBuilder()
            .WithAuthor(Context.User)
            .WithTitle("User Warned")
            .AddField("User", user.Mention)
            .AddField("Moderator", guildUser.Mention)
            .AddField("Reason", reason ?? "No reason provided.")
            .WithColor(Color.Gold)
            .Build();
        await _loggingService.SendLogAsync(Context.Guild.Id, embed: auditLogEmbed);
        
        await _mediator.Send(new ModerateUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason
        });
        
        if (sentDm is not null)
            await ReplyAsync($"User has been warned.");
        else
            await ReplyAsync($"User could not be messaged. The warning has been logged.");
    }
}