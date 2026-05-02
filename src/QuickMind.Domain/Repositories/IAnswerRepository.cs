using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IAnswerRepository
{
    Task<Answer?> GetByIdAsync(Guid id);
    Task<IEnumerable<Answer>> GetByRoundIdAsync(Guid roundId);
    Task<Answer> CreateAsync(Answer answer);
    Task UpdateAsync(Answer answer);
    Task<bool> HasPlayerAnsweredAsync(Guid roundId, Guid playerId, string category);
}
