using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IStatsRepository
{
    Task<PlayerStats?> GetPlayerStatsAsync(Guid userId);
    Task UpsertPlayerStatsAsync(Guid userId, int gamesPlayed, int gamesWon, int gamesLost,
        int totalScore, int currentStreak, int bestStreak, int totalAnswers, int correctAnswers);
    Task IncrementGamesPlayedAsync(Guid userId, int score, bool won);
}

public interface IAchievementRepository
{
    Task<IEnumerable<Achievement>> GetAllAchievementsAsync();
    Task<IEnumerable<Guid>> GetUserAchievementIdsAsync(Guid userId);
    Task AwardAchievementAsync(Guid userId, Guid achievementId);
}

public interface IRoundHistoryRepository
{
    Task<IEnumerable<GameRound>> GetUserRoundsAsync(Guid userId, int limit);
    Task CreateAsync(GameRound round);
}