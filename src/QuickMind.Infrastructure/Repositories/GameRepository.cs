using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GameRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, code, host_id AS HostId, status, max_players AS MaxPlayers,
                   total_rounds AS TotalRounds, current_round AS CurrentRound,
                   time_per_round AS TimePerRound, letter_mode AS LetterMode,
                   validation_type AS ValidationType, created_at AS CreatedAt,
                   started_at AS StartedAt, finished_at AS FinishedAt,
                   is_public AS IsPublic, scheduled_start AS ScheduledStart
            FROM games WHERE id = @Id
            """;
        return await conn.QueryFirstOrDefaultAsync<Game>(sql, new { Id = id });
    }

    public async Task<Game?> GetByCodeAsync(string code)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, code, host_id AS HostId, status, max_players AS MaxPlayers,
                   total_rounds AS TotalRounds, current_round AS CurrentRound,
                   time_per_round AS TimePerRound, letter_mode AS LetterMode,
                   validation_type AS ValidationType, created_at AS CreatedAt,
                   started_at AS StartedAt, finished_at AS FinishedAt,
                   is_public AS IsPublic, scheduled_start AS ScheduledStart
            FROM games WHERE code = @Code
            """;
        return await conn.QueryFirstOrDefaultAsync<Game>(sql, new { Code = code });
    }

    public async Task<IEnumerable<Game>> GetWaitingGamesAsync()
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, code, host_id AS HostId, status, max_players AS MaxPlayers,
                   total_rounds AS TotalRounds, current_round AS CurrentRound,
                   time_per_round AS TimePerRound, letter_mode AS LetterMode,
                   validation_type AS ValidationType, created_at AS CreatedAt,
                   started_at AS StartedAt, finished_at AS FinishedAt,
                   is_public AS IsPublic, scheduled_start AS ScheduledStart
            FROM games WHERE status = 0
            """;
        return await conn.QueryAsync<Game>(sql);
    }

    public async Task<IEnumerable<Game>> GetPublicGamesAsync(DateTime maxScheduledTime)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT g.id, g.code, g.host_id AS HostId, g.status, g.max_players AS MaxPlayers,
                   g.total_rounds AS TotalRounds, g.current_round AS CurrentRound,
                   g.time_per_round AS TimePerRound, g.letter_mode AS LetterMode,
                   g.validation_type AS ValidationType, g.created_at AS CreatedAt,
                   g.started_at AS StartedAt, g.finished_at AS FinishedAt,
                   g.is_public AS IsPublic, g.scheduled_start AS ScheduledStart,
                   COUNT(p.id) AS CurrentPlayers
            FROM games g
            LEFT JOIN players p ON p.game_id = g.id AND p.left_at IS NULL
            WHERE g.is_public = true 
              AND g.status IN (0, 1)
              AND (g.scheduled_start IS NULL OR g.scheduled_start <= @MaxScheduledTime)
            GROUP BY g.id, g.code, g.host_id, g.status, g.max_players,
                     g.total_rounds, g.current_round, g.time_per_round, 
                     g.letter_mode, g.validation_type, g.created_at,
                     g.started_at, g.finished_at, g.is_public, g.scheduled_start
            HAVING COUNT(p.id) < g.max_players
            ORDER BY 
              CASE WHEN g.status = 1 THEN 0 ELSE 1 END,
              COALESCE(g.scheduled_start, g.created_at)
            """;
        return await conn.QueryAsync<Game>(sql, new { MaxScheduledTime = maxScheduledTime });
    }

    public async Task<Game> CreateAsync(Game game)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO games (id, code, host_id, status, max_players, total_rounds,
                               current_round, time_per_round, letter_mode, validation_type, 
                               created_at, is_public, scheduled_start)
            VALUES (@Id, @Code, @HostId, @Status, @MaxPlayers, @TotalRounds,
                    @CurrentRound, @TimePerRound, @LetterMode, @ValidationType, 
                    @CreatedAt, @IsPublic, @ScheduledStart)
            RETURNING id
            """;
        game.Id = await conn.QuerySingleAsync<Guid>(sql, game);
        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            UPDATE games SET status = @Status, current_round = @CurrentRound,
                             started_at = @StartedAt, finished_at = @FinishedAt,
                             is_public = @IsPublic, scheduled_start = @ScheduledStart
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, game);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        await conn.ExecuteAsync("DELETE FROM games WHERE id = @Id", new { Id = id });
    }
}
