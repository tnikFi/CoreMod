using Discord.WebSocket;

namespace Web.Models;

public class RoleDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }

    public static RoleDto FromSocketRole(SocketRole role)
    {
        return new RoleDto
        {
            Id = role.Id.ToString(),
            Name = role.Name,
            Color = role.Color.RawValue != 0 ? role.Color.ToString() : null
        };
    }
}