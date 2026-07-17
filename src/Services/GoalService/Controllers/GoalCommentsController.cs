using GoalService.DTOs;
using GoalService.Services;
using HedefTakip.Shared.Constants;
using HedefTakip.Shared.Models;
using HedefTakip.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoalService.Controllers;

[ApiController]
[Route("api/goal-assignments/{assignmentId:guid}/comments")]
[Authorize]
public class GoalCommentsController : ControllerBase
{
    private readonly IGoalCommentService _commentService;
    private readonly IAuditClient _audit;

    public GoalCommentsController(IGoalCommentService commentService, IAuditClient audit)
    {
        _commentService = commentService;
        _audit = audit;
    }

    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetByAssignment(Guid assignmentId)
    {
        var comments = await _commentService.GetByAssignmentAsync(assignmentId);
        return Ok(ApiResponse<List<GoalCommentDto>>.Ok(comments));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid assignmentId, [FromBody] CreateGoalCommentRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail(Messages.Auth.Forbidden, Messages.Auth.ForbiddenUser, Messages.Auth.ForbiddenUser));

        try
        {
            var comment = await _commentService.CreateAsync(assignmentId, userId.Value, request);
            _audit.Send("GoalService", "Create", "GoalComment", comment.Id.ToString(),
                userId.ToString(), $"Atama: {assignmentId}");
            return CreatedAtAction(nameof(GetByAssignment), new { assignmentId },
                ApiResponse<GoalCommentDto>.Ok(comment, Messages.Comment.Created));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail(Messages.GoalAssignment.NotFound, Messages.GoalAssignment.NotFoundUser, Messages.GoalAssignment.NotFoundUser));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid assignmentId, Guid id)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail(Messages.Auth.Forbidden, Messages.Auth.ForbiddenUser, Messages.Auth.ForbiddenUser));

        var isAdmin = User.IsInRole("Admin");

        try
        {
            var result = await _commentService.DeleteAsync(id, userId.Value, isAdmin);
            if (!result)
                return NotFound(ApiResponse.Fail(Messages.Comment.NotFound, Messages.Comment.NotFoundUser, Messages.Comment.NotFoundUser));

            _audit.Send("GoalService", "Delete", "GoalComment", id.ToString(), userId.ToString());
            return Ok(ApiResponse.OkNoData(Messages.Comment.Deleted));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
