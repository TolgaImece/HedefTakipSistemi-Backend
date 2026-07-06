namespace GoalService.Entities;

public class GoalAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GoalTemplateId { get; set; }
    public Guid UserId { get; set; }
    public Guid PeriodId { get; set; }
    public string Status { get; set; } = GoalAssignmentStatus.NotStarted;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }

    public GoalTemplate GoalTemplate { get; set; } = null!;
    public ICollection<GoalComment> GoalComments { get; set; } = [];
}

public static class GoalAssignmentStatus
{
    public const string NotStarted = "NotStarted";
    public const string InProgress = "InProgress";
    public const string Completed  = "Completed";
    public const string Expired    = "Expired";
    public const string Cancelled  = "Cancelled";

    public static readonly string[] All = [NotStarted, InProgress, Completed, Expired, Cancelled];
}
