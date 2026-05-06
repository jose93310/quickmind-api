using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task<FriendRequest?> GetByIdAsync(Guid id);
    Task<FriendRequest?> GetBetweenUsersAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<FriendRequest>> GetPendingByReceiverAsync(Guid receiverId);
    Task<IEnumerable<FriendRequest>> GetSentByUserAsync(Guid senderId);
    Task CreateAsync(FriendRequest request);
    Task UpdateAsync(FriendRequest request);
    Task DeleteAsync(Guid id);
}
