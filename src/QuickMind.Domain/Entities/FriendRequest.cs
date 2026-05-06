namespace QuickMind.Domain.Entities;

public enum FriendRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class FriendRequest
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
