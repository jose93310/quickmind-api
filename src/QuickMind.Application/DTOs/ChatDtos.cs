namespace QuickMind.Application.DTOs;

public record SendGameMessageDto(
    Guid GameId,
    Guid SenderId,
    string? Text,
    string? MediaUrl,
    string? MediaType
);

public record SendChatMessageDto(
    Guid SenderId,
    Guid ReceiverId,
    string? Text,
    string? MediaUrl,
    string? MediaType
);

public record GameMessageDto(
    Guid Id,
    Guid GameId,
    Guid SenderId,
    string SenderNickname,
    string? Text,
    string? MediaUrl,
    string? MediaType,
    DateTime CreatedAt
);

public record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    string SenderNickname,
    Guid ReceiverId,
    string? Text,
    string? MediaUrl,
    string? MediaType,
    bool IsRead,
    DateTime CreatedAt
);

public record SendReactionDto(
    Guid GameId,
    Guid PlayerId,
    string Reaction
);

public record GameReactionDto(
    Guid Id,
    Guid GameId,
    Guid PlayerId,
    string PlayerNickname,
    string Reaction,
    DateTime CreatedAt
);

public record ConversationPreviewDto(
    Guid UserId,
    string Nickname,
    string? AvatarPath,
    string LastMessage,
    DateTime LastMessageAt,
    int UnreadCount
);
