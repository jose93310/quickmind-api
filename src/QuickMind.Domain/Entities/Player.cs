namespace QuickMind.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public int Score { get; set; }
    public bool IsHost { get; set; }
    public bool IsOnline { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}
