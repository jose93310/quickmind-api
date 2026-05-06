using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task<Game?> GetByCodeAsync(string code);
    Task<IEnumerable<Game>> GetWaitingGamesAsync();
    Task<IEnumerable<Game>> GetPublicGamesAsync(DateTime maxScheduledTime);
    Task<Game> CreateAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(Guid id);
}
