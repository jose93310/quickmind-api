namespace QuickMind.Domain.Entities;

public class GameMessage
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid SenderId { get; set; }
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
