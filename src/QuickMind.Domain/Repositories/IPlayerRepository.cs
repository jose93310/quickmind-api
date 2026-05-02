using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetByGameIdAsync(Guid gameId);
    Task<Player> AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task RemoveAsync(Guid playerId);
    Task<int> GetPlayerCountAsync(Guid gameId);
}
