namespace GoalService.DTOs;

public record GoalTemplateDto(
    Guid Id,
    Guid GoalCategoryId,
    string CategoryName,
    string Title,
    string? Description,
    string GoalType,
    bool IsActive,
    DateTime CreatedTime
);

public record CreateGoalTemplateRequest(
    Guid GoalCategoryId,
    string Title,
    string? Description,
    string GoalType
);

public record UpdateGoalTemplateRequest(
    Guid GoalCategoryId,
    string Title,
    string? Description,
    string GoalType
);
