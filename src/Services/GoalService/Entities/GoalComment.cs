namespace GoalService.Entities;

public class GoalComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GoalAssignmentId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    public GoalAssignment GoalAssignment { get; set; } = null!;
}
