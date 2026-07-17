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
[Route("api/goal-categories")]
[Authorize]
public class GoalCategoriesController : ControllerBase
{
    private readonly IGoalCategoryService _categoryService;
    private readonly IAuditClient _audit;

    public GoalCategoriesController(IGoalCategoryService categoryService, IAuditClient audit)
    {
        _categoryService = categoryService;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true)
    {
        var categories = await _categoryService.GetAllAsync(onlyActive);
        return Ok(ApiResponse<List<GoalCategoryDto>>.Ok(categories));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiResponse.Fail(Messages.GoalCategory.NotFound, Messages.GoalCategory.NotFoundUser, Messages.GoalCategory.NotFoundUser));

        return Ok(ApiResponse<GoalCategoryDto>.Ok(category));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateGoalCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request);
            _audit.Send("GoalService", "Create", "GoalCategory", category.Id.ToString(),
                CurrentUserId, category.Name);
            return CreatedAtAction(nameof(GetById), new { id = category.Id },
                ApiResponse<GoalCategoryDto>.Ok(category, Messages.GoalCategory.Created));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse.Fail(Messages.GoalCategory.AlreadyExists, Messages.GoalCategory.AlreadyExistsUser, Messages.GoalCategory.AlreadyExistsUser));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoalCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request);
            if (category is null)
                return NotFound(ApiResponse.Fail(Messages.GoalCategory.NotFound, Messages.GoalCategory.NotFoundUser, Messages.GoalCategory.NotFoundUser));

            _audit.Send("GoalService", "Update", "GoalCategory", id.ToString(),
                CurrentUserId, category.Name);
            return Ok(ApiResponse<GoalCategoryDto>.Ok(category, Messages.GoalCategory.Updated));
        }
        catch (InvalidOperationException)
        {
            return BadRequest(ApiResponse.Fail(Messages.GoalCategory.AlreadyExists, Messages.GoalCategory.AlreadyExistsUser, Messages.GoalCategory.AlreadyExistsUser));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail(Messages.GoalCategory.NotFound, Messages.GoalCategory.NotFoundUser, Messages.GoalCategory.NotFoundUser));

        _audit.Send("GoalService", "Delete", "GoalCategory", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.GoalCategory.Deleted));
    }
}
