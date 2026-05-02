using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class GameRoundRepository : IGameRoundRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GameRoundRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GameRound?> GetByIdAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, game_id AS GameId, round_number AS RoundNumber,
                   letter, started_at AS StartedAt, ended_at AS EndedAt, status
            FROM game_rounds WHERE id = @Id
            """;
        return await conn.QueryFirstOrDefaultAsync<GameRound>(sql, new { Id = id });
    }

    public async Task<IEnumerable<GameRound>> GetByGameIdAsync(Guid gameId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, game_id AS GameId, round_number AS RoundNumber,
                   letter, started_at AS StartedAt, ended_at AS EndedAt, status
            FROM game_rounds WHERE game_id = @GameId ORDER BY round_number
            """;
        return await conn.QueryAsync<GameRound>(sql, new { GameId = gameId });
    }

    public async Task<GameRound> CreateAsync(GameRound round)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO game_rounds (id, game_id, round_number, letter, started_at, status)
            VALUES (@Id, @GameId, @RoundNumber, @Letter, @StartedAt, @Status)
            RETURNING id
            """;
        round.Id = await conn.QuerySingleAsync<Guid>(sql, round);
        return round;
    }

    public async Task UpdateAsync(GameRound round)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            UPDATE game_rounds SET ended_at = @EndedAt, status = @Status
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, round);
    }
}
