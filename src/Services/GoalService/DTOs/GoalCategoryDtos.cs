namespace GoalService.DTOs;

public record GoalCategoryDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedTime
);

public record CreateGoalCategoryRequest(string Name);

public record UpdateGoalCategoryRequest(string Name);
