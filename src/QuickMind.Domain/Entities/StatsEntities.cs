namespace QuickMind.Domain.Entities;

public class PlayerStats
{
    public Guid UserId { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int TotalScore { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public int TotalAnswers { get; set; }
    public int CorrectAnswers { get; set; }
    public DateTime? LastPlayedAt { get; set; }
}

public class Achievement
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int Points { get; set; }
    public string? RequirementType { get; set; }
    public int? RequirementValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UserAchievement
{
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}