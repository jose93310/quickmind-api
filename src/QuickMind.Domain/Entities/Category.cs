namespace QuickMind.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AgeGroup AgeGroup { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum AgeGroup
{
    Child = 0,
    Teen = 1,
    Adult = 2
}
