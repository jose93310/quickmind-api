namespace QuickMind.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
