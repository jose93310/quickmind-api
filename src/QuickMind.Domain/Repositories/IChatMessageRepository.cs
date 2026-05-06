using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface IChatMessageRepository
{
    Task<IEnumerable<ChatMessage>> GetConversationAsync(Guid userId1, Guid userId2, int limit = 100);
    Task CreateAsync(ChatMessage message);
    Task MarkAsReadAsync(Guid messageId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task DeleteOldMessagesAsync(TimeSpan olderThan);
}
