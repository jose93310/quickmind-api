using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class FriendRepository : IFriendRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FriendRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> AreFriendsAsync(Guid userId, Guid friendId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT 1 FROM friends WHERE user_id = @UserId AND friend_id = @FriendId";
        var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { UserId = userId, FriendId = friendId });
        return result.HasValue;
    }

    public async Task<IEnumerable<User>> GetFriendsAsync(Guid userId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT u.id, u.email, u.nickname, u.name, u.country, u.city, 
                   u.birth_date AS BirthDate, u.gender, u.avatar_path AS AvatarPath,
                   u.password_hash AS PasswordHash, u.is_guest AS IsGuest, u.created_at AS CreatedAt
            FROM friends f
            JOIN users u ON u.id = f.friend_id
            WHERE f.user_id = @UserId
            ORDER BY u.nickname
            """;
        return await conn.QueryAsync<User>(sql, new { UserId = userId });
    }

    public async Task AddFriendAsync(Guid userId, Guid friendId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO friends (user_id, friend_id, created_at)
            VALUES (@UserId, @FriendId, NOW()),
                   (@FriendId, @UserId, NOW())
            ON CONFLICT DO NOTHING
            """;
        await conn.ExecuteAsync(sql, new { UserId = userId, FriendId = friendId });
    }

    public async Task RemoveFriendAsync(Guid userId, Guid friendId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            DELETE FROM friends 
            WHERE (user_id = @UserId AND friend_id = @FriendId)
               OR (user_id = @FriendId AND friend_id = @UserId)
            """;
        await conn.ExecuteAsync(sql, new { UserId = userId, FriendId = friendId });
    }
}
