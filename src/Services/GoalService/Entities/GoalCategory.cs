namespace GoalService.Entities;

public class GoalCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }

    public ICollection<GoalTemplate> GoalTemplates { get; set; } = [];
}
