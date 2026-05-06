using QuickMind.Application.DTOs;

namespace QuickMind.Application.Services;

public interface IFriendService
{
    Task SendFriendRequestAsync(SendFriendRequestDto dto);
    Task RespondToRequestAsync(RespondFriendRequestDto dto);
    Task<List<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);
    Task<List<FriendDto>> GetFriendsAsync(Guid userId);
    Task RemoveFriendAsync(Guid userId, Guid friendId);
    Task<List<UserSearchDto>> SearchUsersAsync(Guid userId, string query);
}
