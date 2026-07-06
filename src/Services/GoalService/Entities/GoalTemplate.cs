namespace GoalService.Entities;

public class GoalTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GoalCategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string GoalType { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }

    public GoalCategory GoalCategory { get; set; } = null!;
    public ICollection<GoalAssignment> GoalAssignments { get; set; } = [];
}
