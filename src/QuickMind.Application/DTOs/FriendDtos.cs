namespace QuickMind.Application.DTOs;

public record FriendRequestDto(
    Guid Id,
    Guid SenderId,
    string SenderNickname,
    string? SenderAvatarPath,
    Guid ReceiverId,
    int Status,
    DateTime CreatedAt
);

public record FriendDto(
    Guid Id,
    string Nickname,
    string? AvatarPath,
    bool IsOnline
);

public record SendFriendRequestDto(
    Guid SenderId,
    string ReceiverNickname
);

public record RespondFriendRequestDto(
    Guid RequestId,
    Guid UserId,
    bool Accept
);

public record UserSearchDto(
    Guid Id,
    string Nickname,
    string? AvatarPath,
    bool IsFriend,
    bool HasPendingRequest
);

public record GameInviteDto(
    Guid GameId,
    string GameCode,
    string HostNickname,
    Guid InvitedUserId
);
