namespace QuickMind.Domain.Entities;

public class RoundVote
{
    public Guid Id { get; set; }
    public Guid AnswerId { get; set; }
    public Guid VoterId { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
}
