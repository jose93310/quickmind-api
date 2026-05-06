namespace QuickMind.Application.DTOs;

public record PlayerStatsDto(
    Guid UserId,
    string Nickname,
    int GamesPlayed,
    int GamesWon,
    int GamesLost,
    int TotalScore,
    int CurrentStreak,
    int BestStreak,
    int TotalAnswers,
    int CorrectAnswers,
    double WinRate,
    double CorrectRate,
    DateTime? LastPlayedAt
);

public record AchievementDto(
    Guid Id,
    string Name,
    string Description,
    string Icon,
    int Points,
    bool IsEarned,
    DateTime? EarnedAt
);

public record CategoryStatsDto(
    string Category,
    int TimesAsked,
    int CorrectAnswers,
    int TotalScore,
    double AverageTime
);

public record RoundHistoryDto(
    Guid GameId,
    string GameCode,
    int RoundNumber,
    string Letter,
    DateTime PlayedAt,
    int Score,
    int CorrectAnswers,
    int TotalAnswers
);

public record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string Nickname,
    string? AvatarPath,
    int TotalScore,
    int GamesWon
);