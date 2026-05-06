using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class FriendRequestRepository : IFriendRequestRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FriendRequestRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<FriendRequest?> GetByIdAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, sender_id AS SenderId, receiver_id AS ReceiverId,
                   status, created_at AS CreatedAt
            FROM friend_requests WHERE id = @Id
            """;
        return await conn.QueryFirstOrDefaultAsync<FriendRequest>(sql, new { Id = id });
    }

    public async Task<FriendRequest?> GetBetweenUsersAsync(Guid userId1, Guid userId2)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, sender_id AS SenderId, receiver_id AS ReceiverId,
                   status, created_at AS CreatedAt
            FROM friend_requests 
            WHERE (sender_id = @UserId1 AND receiver_id = @UserId2)
               OR (sender_id = @UserId2 AND receiver_id = @UserId1)
            ORDER BY created_at DESC
            LIMIT 1
            """;
        return await conn.QueryFirstOrDefaultAsync<FriendRequest>(sql, new { UserId1 = userId1, UserId2 = userId2 });
    }

    public async Task<IEnumerable<FriendRequest>> GetPendingByReceiverAsync(Guid receiverId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, sender_id AS SenderId, receiver_id AS ReceiverId,
                   status, created_at AS CreatedAt
            FROM friend_requests 
            WHERE receiver_id = @ReceiverId AND status = 0
            ORDER BY created_at DESC
            """;
        return await conn.QueryAsync<FriendRequest>(sql, new { ReceiverId = receiverId });
    }

    public async Task<IEnumerable<FriendRequest>> GetSentByUserAsync(Guid senderId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, sender_id AS SenderId, receiver_id AS ReceiverId,
                   status, created_at AS CreatedAt
            FROM friend_requests 
            WHERE sender_id = @SenderId AND status = 0
            ORDER BY created_at DESC
            """;
        return await conn.QueryAsync<FriendRequest>(sql, new { SenderId = senderId });
    }

    public async Task CreateAsync(FriendRequest request)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO friend_requests (id, sender_id, receiver_id, status, created_at)
            VALUES (@Id, @SenderId, @ReceiverId, @Status, @CreatedAt)
            RETURNING id
            """;
        request.Id = await conn.QuerySingleAsync<Guid>(sql, request);
    }

    public async Task UpdateAsync(FriendRequest request)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            UPDATE friend_requests SET status = @Status
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, request);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql = "DELETE FROM friend_requests WHERE id = @Id";
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}
