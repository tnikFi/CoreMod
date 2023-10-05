using Application.Commands.Moderation;
using Application.Interfaces;
using Application.Queries.Moderation;
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

        var moderation = await _mediator.Send(new WarnUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason
        });

        var sentDm = await _moderationMessageService.SendModerationMessageAsync(moderation);

        if (sentDm)
            await ReplyAsync("User has been warned.");
        else
            await ReplyAsync("User could not be messaged. The warning has been logged.");
    }

    [Command("mute")]
    [Summary("Mutes a user.")]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [Alias("timeout")]
    public async Task MuteAsync(
        [Summary("The user to mute.")] IGuildUser user,
        [Summary("The duration of the mute.")] TimeSpan? duration,
        [Summary("The reason for the mute.")] [Remainder]
        string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;

        var moderation = await _mediator.Send(new MuteUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason,
            Duration = duration
        });

        await ReplyAsync("User has been muted.");
    }
    
    [Command("unmute")]
    [Summary("Unmutes a user.")]
    [RequireUserPermission(GuildPermission.MuteMembers)]
    [Alias("untimeout", "removetimeout")]
    public async Task UnmuteAsync(
        [Summary("The user to unmute.")] IGuildUser user,
        [Summary("The reason for the unmute.")] [Remainder]
        string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;

        var moderation = await _mediator.Send(new UnmuteUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason
        });

        await ReplyAsync("User has been unmuted.");
    }
    
    [Command("kick")]
    [Summary("Kicks a user.")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    public async Task KickAsync(
        [Summary("The user to kick.")] IGuildUser user,
        [Summary("The reason for the kick.")] [Remainder]
        string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;

        var moderation = await _mediator.Send(new KickUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason
        });

        await ReplyAsync("User has been kicked.");
    }

    [Command("ban")]
    [Summary("Bans a user.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [Remarks("Use the `tempban` command to ban a user temporarily.")]
    public async Task BanAsync(
        [Summary("The user to ban.")] IGuildUser user,
        [Summary("The reason for the ban.")] [Remainder]
        string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;

        await _mediator.Send(new BanUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason
        });

        await ReplyAsync("User has been banned.");
    }

    [Command("tempban")]
    [Summary("Bans a user temporarily.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [Remarks("The user will be unbanned automatically after the specified duration.")]
    public async Task TempBanAsync(
        [Summary("The user to ban.")] IGuildUser user,
        [Summary("The duration of the ban.")] TimeSpan duration,
        [Summary("The reason for the ban.")] [Remainder]
        string? reason = null)
    {
        if (Context.User is not IGuildUser guildUser)
            return;

        var moderation = await _mediator.Send(new BanUserCommand
        {
            Guild = Context.Guild,
            User = user,
            Moderator = guildUser,
            Reason = reason,
            Duration = duration
        });

        if (moderation.ExpiresAt != null)
        {
            var timestampTag = new TimestampTag(moderation.ExpiresAt.Value, TimestampTagStyles.LongDateTime);
            await ReplyAsync($"User has been banned until {timestampTag}.");
        }
        else
        {
            await ReplyAsync("User has been banned.");
        }
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