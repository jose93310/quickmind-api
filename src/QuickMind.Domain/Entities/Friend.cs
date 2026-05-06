namespace QuickMind.Domain.Entities;

public class Friend
{
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
