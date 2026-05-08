namespace QuickMind.Domain.Entities;

public class Game
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid HostId { get; set; }
    public string? Name { get; set; }
    public GameStatus Status { get; set; }
    public int MaxPlayers { get; set; } = 8;
    public int TotalRounds { get; set; }
    public int CurrentRound { get; set; }
    public int TimePerRound { get; set; }
    public LetterMode LetterMode { get; set; }
    public ValidationType ValidationType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool IsPublic { get; set; } = false;
    public DateTime? ScheduledStart { get; set; }
}

public enum GameStatus
{
    Waiting = 0,
    InProgress = 1,
    Finished = 2,
    Cancelled = 3
}

public enum LetterMode
{
    Random = 0,
    Manual = 1
}

public enum ValidationType
{
    Manual = 0,
    Voting = 1,
    AI = 2
}
