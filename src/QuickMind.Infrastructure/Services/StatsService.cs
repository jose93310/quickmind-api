using QuickMind.Application.DTOs;
using QuickMind.Application.Services;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;
using System.Data;
using Dapper;

namespace QuickMind.Infrastructure.Services;

public class StatsService : IStatsService
{
    private readonly IStatsRepository _statsRepo;
    private readonly IAchievementRepository _achievementRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDbConnectionFactory _connectionFactory;

    public StatsService(
        IStatsRepository statsRepo,
        IAchievementRepository achievementRepo,
        IUserRepository userRepo,
        IDbConnectionFactory connectionFactory)
    {
        _statsRepo = statsRepo;
        _achievementRepo = achievementRepo;
        _userRepo = userRepo;
        _connectionFactory = connectionFactory;
    }

    public async Task<PlayerStatsDto> GetPlayerStatsAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var stats = await _statsRepo.GetPlayerStatsAsync(userId);

        return new PlayerStatsDto(
            userId,
            user.Nickname,
            stats?.GamesPlayed ?? 0,
            stats?.GamesWon ?? 0,
            stats?.GamesLost ?? 0,
            stats?.TotalScore ?? 0,
            stats?.CurrentStreak ?? 0,
            stats?.BestStreak ?? 0,
            stats?.TotalAnswers ?? 0,
            stats?.CorrectAnswers ?? 0,
            stats?.GamesPlayed > 0 ? (double)stats.GamesWon / stats.GamesPlayed * 100 : 0,
            stats?.TotalAnswers > 0 ? (double)stats.CorrectAnswers / stats.TotalAnswers * 100 : 0,
            stats?.LastPlayedAt
        );
    }

    public async Task<List<AchievementDto>> GetAchievementsAsync(Guid userId)
    {
        var achievements = await _achievementRepo.GetAllAchievementsAsync();
        var userAchievementIds = (await _achievementRepo.GetUserAchievementIdsAsync(userId)).ToHashSet();

        var result = new List<AchievementDto>();
        foreach (var a in achievements)
        {
            result.Add(new AchievementDto(
                a.Id,
                a.Name,
                a.Description ?? "",
                a.Icon ?? "🏆",
                a.Points,
                userAchievementIds.Contains(a.Id),
                null
            ));
        }

        return result;
    }

    public async Task<List<CategoryStatsDto>> GetCategoryStatsAsync(Guid userId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT a.category AS Category, COUNT(*) AS TimesAsked,
                   SUM(CASE WHEN a.is_valid = true THEN 1 ELSE 0 END) AS CorrectAnswers,
                   SUM(a.points) AS TotalScore,
                   AVG(EXTRACT(EPOCH FROM (a.created_at - r.started_at))) AS AverageTime
            FROM answers a
            JOIN game_rounds r ON r.id = a.round_id
            JOIN players p ON p.id = a.player_id
            WHERE p.user_id = @UserId
            GROUP BY a.category
            ORDER BY TimesAsked DESC
            """;
        var results = await conn.QueryAsync<CategoryStatsDto>(sql, new { UserId = userId });
        return results.ToList();
    }

    public async Task<List<RoundHistoryDto>> GetRoundHistoryAsync(Guid userId, int limit = 20)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT gr.game_id AS GameId, g.code AS GameCode, gr.round_number AS RoundNumber,
                   gr.letter, gr.started_at AS PlayedAt,
                   COALESCE(SUM(a.points), 0) AS Score,
                   COUNT(DISTINCT CASE WHEN a.is_valid = true THEN a.id END) AS CorrectAnswers,
                   COUNT(a.id) AS TotalAnswers
            FROM game_rounds gr
            JOIN games g ON g.id = gr.game_id
            JOIN players p ON p.game_id = g.id AND p.user_id = @UserId
            LEFT JOIN answers a ON a.round_id = gr.id AND a.player_id = p.id
            GROUP BY gr.id, g.code, gr.round_number, gr.letter, gr.started_at
            ORDER BY gr.started_at DESC
            LIMIT @Limit
            """;
        var results = await conn.QueryAsync<RoundHistoryDto>(sql, new { UserId = userId, Limit = limit });
        return results.ToList();
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 10)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT u.id AS UserId, u.nickname AS Nickname, u.avatar_path AS AvatarPath,
                   ps.total_score AS TotalScore, ps.games_won AS GamesWon,
                   ROW_NUMBER() OVER (ORDER BY ps.total_score DESC) AS Rank
            FROM player_stats ps
            JOIN users u ON u.id = ps.user_id
            ORDER BY ps.total_score DESC
            LIMIT @Limit
            """;
        var results = await conn.QueryAsync<LeaderboardEntryDto>(sql, new { Limit = limit });
        return results.ToList();
    }

    public async Task UpdateStatsAfterGameAsync(Guid userId, int score, bool won)
    {
        await _statsRepo.IncrementGamesPlayedAsync(userId, score, won);
        await CheckAndAwardAchievementsAsync(userId);
    }

    public async Task CheckAndAwardAchievementsAsync(Guid userId)
    {
        var stats = await _statsRepo.GetPlayerStatsAsync(userId);
        if (stats == null) return;

        var achievements = await _achievementRepo.GetAllAchievementsAsync();
        var userAchievements = await _achievementRepo.GetUserAchievementIdsAsync(userId);
        var earnedIds = userAchievements.ToHashSet();

        foreach (var achievement in achievements)
        {
            if (earnedIds.Contains(achievement.Id)) continue;

            bool earned = achievement.RequirementType switch
            {
                "games_played" => stats.GamesPlayed >= (achievement.RequirementValue ?? 0),
                "games_won" => stats.GamesWon >= (achievement.RequirementValue ?? 0),
                "total_score" => stats.TotalScore >= (achievement.RequirementValue ?? 0),
                "streak" => stats.BestStreak >= (achievement.RequirementValue ?? 0),
                _ => false
            };

            if (earned)
            {
                await _achievementRepo.AwardAchievementAsync(userId, achievement.Id);
            }
        }
    }
}