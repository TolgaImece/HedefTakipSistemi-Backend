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
[Route("api/goal-assignments")]
[Authorize]
public class GoalAssignmentsController : ControllerBase
{
    private readonly IGoalAssignmentService _assignmentService;
    private readonly IAuditClient _audit;

    public GoalAssignmentsController(IGoalAssignmentService assignmentService, IAuditClient audit)
    {
        _assignmentService = assignmentService;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll()
    {
        var assignments = await _assignmentService.GetAllAsync();
        return Ok(ApiResponse<List<GoalAssignmentDto>>.Ok(assignments));
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var assignments = await _assignmentService.GetByUserAsync(userId);
        return Ok(ApiResponse<List<GoalAssignmentDto>>.Ok(assignments));
    }

    [HttpGet("by-period/{periodId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetByPeriod(Guid periodId)
    {
        var assignments = await _assignmentService.GetByPeriodAsync(periodId);
        return Ok(ApiResponse<List<GoalAssignmentDto>>.Ok(assignments));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        if (assignment is null)
            return NotFound(ApiResponse.Fail(Messages.GoalAssignment.NotFound, Messages.GoalAssignment.NotFoundUser, Messages.GoalAssignment.NotFoundUser));

        return Ok(ApiResponse<GoalAssignmentDto>.Ok(assignment));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateGoalAssignmentRequest request)
    {
        try
        {
            var assignment = await _assignmentService.CreateAsync(request);
            _audit.Send("GoalService", "Create", "GoalAssignment", assignment.Id.ToString(),
                CurrentUserId, $"Şablon: {assignment.TemplateTitle}, Kullanıcı: {assignment.UserId}, Dönem: {assignment.PeriodId}");
            return CreatedAtAction(nameof(GetById), new { id = assignment.Id },
                ApiResponse<GoalAssignmentDto>.Ok(assignment, Messages.GoalAssignment.Created));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail(Messages.GoalTemplate.NotFound, Messages.GoalTemplate.NotFoundUser, Messages.GoalTemplate.NotFoundUser));
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateGoalAssignmentStatusRequest request)
    {
        try
        {
            var assignment = await _assignmentService.UpdateStatusAsync(id, request);
            if (assignment is null)
                return NotFound(ApiResponse.Fail(Messages.GoalAssignment.NotFound, Messages.GoalAssignment.NotFoundUser, Messages.GoalAssignment.NotFoundUser));

            _audit.Send("GoalService", "StatusChange", "GoalAssignment", id.ToString(),
                CurrentUserId, $"Yeni durum: {request.Status}");
            return Ok(ApiResponse<GoalAssignmentDto>.Ok(assignment, Messages.GoalAssignment.StatusUpdated));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse.Fail(Messages.GoalAssignment.InvalidStatus, Messages.GoalAssignment.InvalidStatusUser, Messages.GoalAssignment.InvalidStatusUser));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _assignmentService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail(Messages.GoalAssignment.NotFound, Messages.GoalAssignment.NotFoundUser, Messages.GoalAssignment.NotFoundUser));

        _audit.Send("GoalService", "Delete", "GoalAssignment", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.GoalAssignment.Deleted));
    }
}
