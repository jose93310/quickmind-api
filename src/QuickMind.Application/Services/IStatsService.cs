using QuickMind.Application.DTOs;

namespace QuickMind.Application.Services;

public interface IStatsService
{
    Task<PlayerStatsDto> GetPlayerStatsAsync(Guid userId);
    Task<List<AchievementDto>> GetAchievementsAsync(Guid userId);
    Task<List<CategoryStatsDto>> GetCategoryStatsAsync(Guid userId);
    Task<List<RoundHistoryDto>> GetRoundHistoryAsync(Guid userId, int limit = 20);
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10);
    Task UpdateStatsAfterGameAsync(Guid userId, int score, bool won);
    Task CheckAndAwardAchievementsAsync(Guid userId);
}