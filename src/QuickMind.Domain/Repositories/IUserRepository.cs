using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByNicknameAsync(string nickname);
    Task<IEnumerable<User>> SearchByNicknameAsync(string query, Guid excludeUserId, int limit = 20);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
}
