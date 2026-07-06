namespace GoalService.DTOs;

public record GoalAssignmentDto(
    Guid Id,
    Guid GoalTemplateId,
    string TemplateTitle,
    string GoalType,
    string CategoryName,
    Guid UserId,
    Guid PeriodId,
    string Status,
    bool IsActive,
    DateTime CreatedTime
);

public record CreateGoalAssignmentRequest(
    Guid GoalTemplateId,
    Guid UserId,
    Guid PeriodId
);

public record UpdateGoalAssignmentStatusRequest(string Status);
