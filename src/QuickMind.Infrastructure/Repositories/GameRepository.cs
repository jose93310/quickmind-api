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
                   started_at AS StartedAt, finished_at AS FinishedAt
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
                   started_at AS StartedAt, finished_at AS FinishedAt
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
                   started_at AS StartedAt, finished_at AS FinishedAt
            FROM games WHERE status = 0
            """;
        return await conn.QueryAsync<Game>(sql);
    }

    public async Task<Game> CreateAsync(Game game)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO games (id, code, host_id, status, max_players, total_rounds,
                               current_round, time_per_round, letter_mode, validation_type, created_at)
            VALUES (@Id, @Code, @HostId, @Status, @MaxPlayers, @TotalRounds,
                    @CurrentRound, @TimePerRound, @LetterMode, @ValidationType, @CreatedAt)
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
                             started_at = @StartedAt, finished_at = @FinishedAt
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
