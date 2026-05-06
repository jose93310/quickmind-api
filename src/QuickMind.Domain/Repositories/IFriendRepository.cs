using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IFriendRepository
{
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId);
    Task<IEnumerable<User>> GetFriendsAsync(Guid userId);
    Task AddFriendAsync(Guid userId, Guid friendId);
    Task RemoveFriendAsync(Guid userId, Guid friendId);
}
