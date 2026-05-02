using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PlayerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, game_id AS GameId, user_id AS UserId, nickname,
                   avatar_path AS AvatarPath, score, is_host AS IsHost,
                   is_online AS IsOnline, joined_at AS JoinedAt, left_at AS LeftAt
            FROM players WHERE id = @Id
            """;
        return await conn.QueryFirstOrDefaultAsync<Player>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Player>> GetByGameIdAsync(Guid gameId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, game_id AS GameId, user_id AS UserId, nickname,
                   avatar_path AS AvatarPath, score, is_host AS IsHost,
                   is_online AS IsOnline, joined_at AS JoinedAt, left_at AS LeftAt
            FROM players WHERE game_id = @GameId ORDER BY score DESC
            """;
        return await conn.QueryAsync<Player>(sql, new { GameId = gameId });
    }

    public async Task<Player> AddAsync(Player player)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO players (id, game_id, user_id, nickname, avatar_path, score, is_host, is_online, joined_at)
            VALUES (@Id, @GameId, @UserId, @Nickname, @AvatarPath, @Score, @IsHost, @IsOnline, @JoinedAt)
            RETURNING id
            """;
        player.Id = await conn.QuerySingleAsync<Guid>(sql, player);
        return player;
    }

    public async Task UpdateAsync(Player player)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            UPDATE players SET score = @Score, is_online = @IsOnline, left_at = @LeftAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, player);
    }

    public async Task RemoveAsync(Guid playerId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        await conn.ExecuteAsync("DELETE FROM players WHERE id = @Id", new { Id = playerId });
    }

    public async Task<int> GetPlayerCountAsync(Guid gameId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        return await conn.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM players WHERE game_id = @GameId", new { GameId = gameId });
    }
}
