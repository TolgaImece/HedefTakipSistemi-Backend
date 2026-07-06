namespace GoalService.DTOs;

public record GoalCommentDto(
    Guid Id,
    Guid GoalAssignmentId,
    Guid UserId,
    string Content,
    DateTime CreatedTime
);

public record CreateGoalCommentRequest(string Content);
