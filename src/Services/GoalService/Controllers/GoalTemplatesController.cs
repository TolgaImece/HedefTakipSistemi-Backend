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
[Route("api/goal-templates")]
[Authorize]
public class GoalTemplatesController : ControllerBase
{
    private readonly IGoalTemplateService _templateService;
    private readonly IAuditClient _audit;

    public GoalTemplatesController(IGoalTemplateService templateService, IAuditClient audit)
    {
        _templateService = templateService;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true)
    {
        var templates = await _templateService.GetAllAsync(onlyActive);
        return Ok(ApiResponse<List<GoalTemplateDto>>.Ok(templates));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template is null)
            return NotFound(ApiResponse.Fail(Messages.GoalTemplate.NotFound, Messages.GoalTemplate.NotFoundUser, Messages.GoalTemplate.NotFoundUser));

        return Ok(ApiResponse<GoalTemplateDto>.Ok(template));
    }

    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var templates = await _templateService.GetByCategoryAsync(categoryId);
        return Ok(ApiResponse<List<GoalTemplateDto>>.Ok(templates));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateGoalTemplateRequest request)
    {
        try
        {
            var template = await _templateService.CreateAsync(request);
            _audit.Send("GoalService", "CreateGoalTemplate", "GoalTemplate", template.Id.ToString(),
                CurrentUserId, template.Title);
            return CreatedAtAction(nameof(GetById), new { id = template.Id },
                ApiResponse<GoalTemplateDto>.Ok(template, Messages.GoalTemplate.Created));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail(Messages.GoalCategory.NotFound, Messages.GoalCategory.NotFoundUser, Messages.GoalCategory.NotFoundUser));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoalTemplateRequest request)
    {
        try
        {
            var template = await _templateService.UpdateAsync(id, request);
            if (template is null)
                return NotFound(ApiResponse.Fail(Messages.GoalTemplate.NotFound, Messages.GoalTemplate.NotFoundUser, Messages.GoalTemplate.NotFoundUser));

            _audit.Send("GoalService", "UpdateGoalTemplate", "GoalTemplate", id.ToString(),
                CurrentUserId, template.Title);
            return Ok(ApiResponse<GoalTemplateDto>.Ok(template, Messages.GoalTemplate.Updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail(Messages.GoalCategory.NotFound, Messages.GoalCategory.NotFoundUser, Messages.GoalCategory.NotFoundUser));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _templateService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail(Messages.GoalTemplate.NotFound, Messages.GoalTemplate.NotFoundUser, Messages.GoalTemplate.NotFoundUser));

        _audit.Send("GoalService", "DeleteGoalTemplate", "GoalTemplate", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.GoalTemplate.Deleted));
    }
}
