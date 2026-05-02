using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IGameRoundRepository
{
    Task<GameRound?> GetByIdAsync(Guid id);
    Task<IEnumerable<GameRound>> GetByGameIdAsync(Guid gameId);
    Task<GameRound> CreateAsync(GameRound round);
    Task UpdateAsync(GameRound round);
}
