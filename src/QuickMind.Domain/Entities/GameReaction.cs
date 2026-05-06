namespace QuickMind.Domain.Entities;

public class GameReaction
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public string Reaction { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
