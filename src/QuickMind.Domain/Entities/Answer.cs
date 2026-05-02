namespace QuickMind.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; }
    public Guid RoundId { get; set; }
    public Guid PlayerId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool? IsValid { get; set; }
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }
}
