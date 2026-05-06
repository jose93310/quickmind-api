using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IGameMessageRepository
{
    Task<IEnumerable<GameMessage>> GetByGameIdAsync(Guid gameId, int limit = 100);
    Task CreateAsync(GameMessage message);
    Task DeleteOldMessagesAsync(TimeSpan olderThan);
}
