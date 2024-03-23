using Discord;
using Discord.WebSocket;

namespace Web.Models;

public class UserDto
{
    public required string Id { get; set; }
    public string? Username { get; set; }
    public string? Nickname { get; set; }
    public required string Icon { get; set; }
    public string? Color { get; set; }

    public static UserDto FromUser(IUser user)
    {
        var dto = new UserDto
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Icon = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
        };

        if (user is IGuildUser guildUser) dto.Nickname = guildUser.Nickname;

        if (user is SocketGuildUser socketGuildUser)
            dto.Color = socketGuildUser.Roles
                .Where(r => r.Color.RawValue != 0)
                .MaxBy(r => r.Position)?.Color.ToString();

        return dto;
    }
}