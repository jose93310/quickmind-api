using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ChatMessageRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ChatMessage>> GetConversationAsync(Guid userId1, Guid userId2, int limit = 100)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, sender_id AS SenderId, receiver_id AS ReceiverId,
                   text, media_url AS MediaUrl, media_type AS MediaType,
                   is_read AS IsRead, created_at AS CreatedAt
            FROM chat_messages
            WHERE (sender_id = @UserId1 AND receiver_id = @UserId2)
               OR (sender_id = @UserId2 AND receiver_id = @UserId1)
            ORDER BY created_at DESC
            LIMIT @Limit
            """;
        return await conn.QueryAsync<ChatMessage>(sql, new { UserId1 = userId1, UserId2 = userId2, Limit = limit });
    }

    public async Task CreateAsync(ChatMessage message)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO chat_messages (id, sender_id, receiver_id, text, media_url, media_type, is_read, created_at)
            VALUES (@Id, @SenderId, @ReceiverId, @Text, @MediaUrl, @MediaType, @IsRead, @CreatedAt)
            """;
        await conn.ExecuteAsync(sql, message);
    }

    public async Task MarkAsReadAsync(Guid messageId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql = "UPDATE chat_messages SET is_read = TRUE WHERE id = @Id";
        await conn.ExecuteAsync(sql, new { Id = messageId });
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT COUNT(*) FROM chat_messages WHERE receiver_id = @UserId AND is_read = FALSE";
        return await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task DeleteOldMessagesAsync(TimeSpan olderThan)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var cutoff = DateTime.UtcNow.Subtract(olderThan);
        const string sql = "DELETE FROM chat_messages WHERE created_at < @Cutoff";
        await conn.ExecuteAsync(sql, new { Cutoff = cutoff });
    }
}
