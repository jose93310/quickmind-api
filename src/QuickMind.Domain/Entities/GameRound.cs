namespace QuickMind.Domain.Entities;

public class GameRound
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public int RoundNumber { get; set; }
    public string Letter { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public RoundStatus Status { get; set; }
}

public enum RoundStatus
{
    Pending = 0,
    InProgress = 1,
    Finished = 2
}
