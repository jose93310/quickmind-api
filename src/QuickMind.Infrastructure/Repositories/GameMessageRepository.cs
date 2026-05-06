using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class GameMessageRepository : IGameMessageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GameMessageRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<GameMessage>> GetByGameIdAsync(Guid gameId, int limit = 100)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, game_id AS GameId, sender_id AS SenderId,
                   text, media_url AS MediaUrl, media_type AS MediaType,
                   created_at AS CreatedAt
            FROM game_messages
            WHERE game_id = @GameId
            ORDER BY created_at DESC
            LIMIT @Limit
            """;
        return await conn.QueryAsync<GameMessage>(sql, new { GameId = gameId, Limit = limit });
    }

    public async Task CreateAsync(GameMessage message)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO game_messages (id, game_id, sender_id, text, media_url, media_type, created_at)
            VALUES (@Id, @GameId, @SenderId, @Text, @MediaUrl, @MediaType, @CreatedAt)
            """;
        await conn.ExecuteAsync(sql, message);
    }

    public async Task DeleteOldMessagesAsync(TimeSpan olderThan)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var cutoff = DateTime.UtcNow.Subtract(olderThan);
        const string sql = "DELETE FROM game_messages WHERE created_at < @Cutoff";
        await conn.ExecuteAsync(sql, new { Cutoff = cutoff });
    }
}
