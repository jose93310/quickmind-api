using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class StatsRepository : IStatsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StatsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PlayerStats?> GetPlayerStatsAsync(Guid userId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT user_id AS UserId, games_played AS GamesPlayed, games_won AS GamesWon,
                   games_lost AS GamesLost, total_score AS TotalScore, current_streak AS CurrentStreak,
                   best_streak AS BestStreak, total_answers AS TotalAnswers, correct_answers AS CorrectAnswers,
                   last_played_at AS LastPlayedAt
            FROM player_stats WHERE user_id = @UserId
            """;
        return await conn.QueryFirstOrDefaultAsync<PlayerStats>(sql, new { UserId = userId });
    }

    public async Task UpsertPlayerStatsAsync(Guid userId, int gamesPlayed, int gamesWon, int gamesLost,
        int totalScore, int currentStreak, int bestStreak, int totalAnswers, int correctAnswers)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO player_stats (user_id, games_played, games_won, games_lost, total_score,
                                       current_streak, best_streak, total_answers, correct_answers, last_played_at)
            VALUES (@UserId, @GamesPlayed, @GamesWon, @GamesLost, @TotalScore,
                    @CurrentStreak, @BestStreak, @TotalAnswers, @CorrectAnswers, NOW())
            ON CONFLICT (user_id) DO UPDATE SET
                games_played = @GamesPlayed,
                games_won = @GamesWon,
                games_lost = @GamesLost,
                total_score = @TotalScore,
                current_streak = @CurrentStreak,
                best_streak = @BestStreak,
                total_answers = @TotalAnswers,
                correct_answers = @CorrectAnswers,
                last_played_at = NOW()
            """;
        await conn.ExecuteAsync(sql, new {
            UserId = userId, GamesPlayed = gamesPlayed, GamesWon = gamesWon, GamesLost = gamesLost,
            TotalScore = totalScore, CurrentStreak = currentStreak, BestStreak = bestStreak,
            TotalAnswers = totalAnswers, CorrectAnswers = correctAnswers
        });
    }

    public async Task IncrementGamesPlayedAsync(Guid userId, int score, bool won)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO player_stats (user_id, games_played, games_won, games_lost, total_score,
                                       current_streak, best_streak, last_played_at)
            VALUES (@UserId, 1, @Won, @Lost, @Score, 1, 1, NOW())
            ON CONFLICT (user_id) DO UPDATE SET
                games_played = player_stats.games_played + 1,
                games_won = player_stats.games_won + @Won,
                games_lost = player_stats.games_lost + @Lost,
                total_score = player_stats.total_score + @Score,
                current_streak = CASE WHEN @Won = 1 THEN player_stats.current_streak + 1 ELSE 0 END,
                best_streak = GREATEST(player_stats.best_streak, CASE WHEN @Won = 1 THEN player_stats.current_streak + 1 ELSE 0 END),
                last_played_at = NOW()
            """;
        await conn.ExecuteAsync(sql, new { UserId = userId, Won = won ? 1 : 0, Lost = won ? 0 : 1, Score = score });
    }
}

public class AchievementRepository : IAchievementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AchievementRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Achievement>> GetAllAchievementsAsync()
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, name, description, icon, points, requirement_type AS RequirementType,
                   requirement_value AS RequirementValue, created_at AS CreatedAt
            FROM achievements ORDER BY points
            """;
        return await conn.QueryAsync<Achievement>(sql);
    }

    public async Task<IEnumerable<Guid>> GetUserAchievementIdsAsync(Guid userId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT achievement_id FROM user_achievements WHERE user_id = @UserId";
        return await conn.QueryAsync<Guid>(sql, new { UserId = userId });
    }

    public async Task AwardAchievementAsync(Guid userId, Guid achievementId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO user_achievements (user_id, achievement_id, earned_at)
            VALUES (@UserId, @AchievementId, NOW())
            ON CONFLICT DO NOTHING
            """;
        await conn.ExecuteAsync(sql, new { UserId = userId, AchievementId = achievementId });
    }
}

public class RoundHistoryRepository : IRoundHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoundHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<GameRound>> GetUserRoundsAsync(Guid userId, int limit)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT gr.id, gr.game_id AS GameId, gr.round_number AS RoundNumber, gr.letter,
                   gr.started_at AS StartedAt, gr.ended_at AS EndedAt, gr.status
            FROM game_rounds gr
            JOIN players p ON p.game_id = gr.game_id
            WHERE p.user_id = @UserId
            ORDER BY gr.started_at DESC
            LIMIT @Limit
            """;
        return await conn.QueryAsync<GameRound>(sql, new { UserId = userId, Limit = limit });
    }

    public async Task CreateAsync(GameRound round)
    {
        // Not needed for now - rounds are created by the game service
    }
}