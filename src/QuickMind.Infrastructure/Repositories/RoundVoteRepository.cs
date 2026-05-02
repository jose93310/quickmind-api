using Dapper;
using QuickMind.Domain.Entities;
using QuickMind.Domain.Repositories;
using QuickMind.Infrastructure.Data;

namespace QuickMind.Infrastructure.Repositories;

public class RoundVoteRepository : IRoundVoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoundVoteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RoundVote> CreateAsync(RoundVote vote)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            INSERT INTO round_votes (id, answer_id, voter_id, is_valid, created_at)
            VALUES (@Id, @AnswerId, @VoterId, @IsValid, @CreatedAt)
            ON CONFLICT (answer_id, voter_id) DO UPDATE SET is_valid = @IsValid
            RETURNING id
            """;
        vote.Id = await conn.QuerySingleAsync<Guid>(sql, vote);
        return vote;
    }

    public async Task<IEnumerable<RoundVote>> GetByAnswerIdAsync(Guid answerId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT id, answer_id AS AnswerId, voter_id AS VoterId,
                   is_valid AS IsValid, created_at AS CreatedAt
            FROM round_votes WHERE answer_id = @AnswerId
            """;
        return await conn.QueryAsync<RoundVote>(sql, new { AnswerId = answerId });
    }

    public async Task<Dictionary<Guid, bool>> GetVoteResultsAsync(Guid answerId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var votes = await conn.QueryAsync<(Guid voterId, bool isValid)>(
            "SELECT voter_id, is_valid FROM round_votes WHERE answer_id = @AnswerId",
            new { AnswerId = answerId });

        return votes.ToDictionary(v => v.voterId, v => v.isValid);
    }
}
