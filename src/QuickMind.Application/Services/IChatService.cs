using QuickMind.Application.DTOs;

namespace QuickMind.Application.Services;

public interface IChatService
{
    Task<GameMessageDto> SendGameMessageAsync(SendGameMessageDto dto);
    Task<List<GameMessageDto>> GetGameMessagesAsync(Guid gameId);
    Task<ChatMessageDto> SendChatMessageAsync(SendChatMessageDto dto);
    Task<List<ChatMessageDto>> GetConversationAsync(Guid userId1, Guid userId2);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid messageId);
    Task<GameReactionDto> SendReactionAsync(SendReactionDto dto);
    Task CleanupOldMessagesAsync();
}
