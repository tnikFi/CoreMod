using Application.Commands.Moderation.ModerateUser;
using Application.Interfaces;
using Application.Queries.Moderation.GetModerations;
using Common.Utils;
using Discord;
using Discord.Commands;
using Domain.Attributes;
using Domain.Enums;
using MediatR;

namespace Web.Discord.Modules;

[Name("Moderation")]
[Summary("Moderation commands")]
[RequireContext(ContextType.Guild)]
public class ModerationModule : ModuleBase<SocketCommandContext>
{
    private readonly ILoggingService _loggingService;
    private readonly IMediator _mediator;
    private readonly IModerationMessageService _moderationMessageService;

    public ModerationModule(IMediator mediator, ILoggingService loggingService,
        IModerationMessageService moderationMessageService)
    {
        _mediator = mediator;
        _loggingService = loggingService;
        _moderationMessageService = moderationMessageService;
    }

    [Command("warn")]
    [Summary("Warns a user.")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task WarnAsync(
        [Summary("The user to warn.")] IGuildUser user,
        [Summary("The reason for the warning.")] [Remainder]
        string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;

        var moderation = await _mediator.Send(new ModerateUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason,
            Type = ModerationType.Warning,
            SendModerationMessage = false // We need to know if the DM was sent or not so we'll do it manually
        });

        var sentDm = await _moderationMessageService.SendModerationMessageAsync(moderation);

        if (sentDm)
            await ReplyAsync("User has been warned.");
        else
            await ReplyAsync("User could not be messaged. The warning has been logged.");
    }

    [Command("purge")]
    [Summary("Deletes multiple messages from the current channel.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task PurgeAsync(
        [Summary("The number of messages to delete.")]
        int count)
    {
        var messages = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
        var messageArray = messages.ToArray();
        await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messageArray);
        await ReplyAsync($"Deleted {messageArray.Length - 1} messages.");
        await _loggingService.SendLogAsync(Context.Guild.Id, embed: new EmbedBuilder()
            .WithAuthor(Context.User)
            .WithTitle($"Purged {messageArray.Length - 1} messages")
            .WithDescription(
                $"Purged {messageArray.Length - 1} messages in {((ITextChannel) Context.Channel).Mention}.")
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .Build());
    }

    [Command("modlogs")]
    [Summary("Gets the moderation logs for a user.")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task GetModLogsAsync(
        [Summary("The user to get the moderation logs for. Leave blank to list moderation logs for all users.")]
        IGuildUser? user = null)
    {
        var modLogs = await _mediator.Send(new GetModerationsQuery
        {
            Guild = Context.Guild,
            User = user
        });

        if (modLogs.Any())
        {
            // Show an embed with the number of found mod logs before sending the actual mod logs.
            var summary = new EmbedBuilder()
                .WithTitle("Moderation Logs")
                .WithDescription(user is null
                    ? $"Found {modLogs.Count()} moderation logs."
                    : $"Found {modLogs.Count()} moderation logs for {user.Mention}.")
                .Build();

            await ReplyAsync(embed: summary);

            foreach (var modLog in modLogs)
            {
                var typeLabel = modLog.Type switch
                {
                    ModerationType.Ban => "Ban",
                    ModerationType.Kick => "Kick",
                    ModerationType.Mute => "Mute",
                    ModerationType.Warning => "Warning",
                    ModerationType.Unmute => "Unmute",
                    ModerationType.Unban => "Unban",
                    _ => throw new ArgumentOutOfRangeException()
                };

                var embed = new EmbedBuilder()
                    .WithAuthor(Context.Guild.GetUser(modLog.ModeratorId))
                    .WithTitle(typeLabel)
                    .WithDescription(modLog.Reason ?? "No reason provided.")
                    .AddField("User", Context.Guild.GetUser(modLog.UserId).Mention)
                    .AddField("Moderator", Context.Guild.GetUser(modLog.ModeratorId).Mention)
                    .WithTimestamp(modLog.Timestamp.ToLocalTime())
                    .WithFooter($"Case #{modLog.Id}")
                    .WithColor(EnumUtils.GetAttributeValue<EmbedColorAttribute>(modLog.Type)?.Color ?? Color.Default)
                    .Build();

                await ReplyAsync(embed: embed);
            }
        }
        else
        {
            await ReplyAsync("No moderation logs found.");
        }
    }
}