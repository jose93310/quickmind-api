using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IRoundVoteRepository
{
    Task<RoundVote> CreateAsync(RoundVote vote);
    Task<IEnumerable<RoundVote>> GetByAnswerIdAsync(Guid answerId);
    Task<Dictionary<Guid, bool>> GetVoteResultsAsync(Guid answerId);
}
