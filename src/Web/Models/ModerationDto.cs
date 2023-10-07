namespace Web.Models;

public class ModerationDto
{
    public required int Id { get; set; }
    public required string Type { get; set; }
    public string? Reason { get; set; }
    public required ulong UserId { get; set; }
    public ulong? ModeratorId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}