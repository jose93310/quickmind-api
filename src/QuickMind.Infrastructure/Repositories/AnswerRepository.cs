using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class AnswerRepository : IAnswerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnswerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Answer?> GetByIdAsync(Guid id)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, round_id AS RoundId, player_id AS PlayerId,
                   category, text, is_valid AS IsValid, points, created_at AS CreatedAt
            FROM answers WHERE id = @Id
            """;
        return await conn.QueryFirstOrDefaultAsync<Answer>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Answer>> GetByRoundIdAsync(Guid roundId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, round_id AS RoundId, player_id AS PlayerId,
                   category, text, is_valid AS IsValid, points, created_at AS CreatedAt
            FROM answers WHERE round_id = @RoundId
            """;
        return await conn.QueryAsync<Answer>(sql, new { RoundId = roundId });
    }

    public async Task<Answer> CreateAsync(Answer answer)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO answers (id, round_id, player_id, category, text, is_valid, points, created_at)
            VALUES (@Id, @RoundId, @PlayerId, @Category, @Text, @IsValid, @Points, @CreatedAt)
            RETURNING id
            """;
        answer.Id = await conn.QuerySingleAsync<Guid>(sql, answer);
        return answer;
    }

    public async Task UpdateAsync(Answer answer)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            UPDATE answers SET is_valid = @IsValid, points = @Points
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, answer);
    }

    public async Task<bool> HasPlayerAnsweredAsync(Guid roundId, Guid playerId, string category)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var count = await conn.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM answers WHERE round_id = @RoundId AND player_id = @PlayerId AND category = @Category",
            new { RoundId = roundId, PlayerId = playerId, Category = category });
        return count > 0;
    }
}
