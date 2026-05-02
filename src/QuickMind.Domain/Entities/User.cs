namespace QuickMind.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? AvatarPath { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsGuest { get; set; }
    public DateTime CreatedAt { get; set; }
}
