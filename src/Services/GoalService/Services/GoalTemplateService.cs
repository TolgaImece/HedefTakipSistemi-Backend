using GoalService.Data;
using GoalService.DTOs;
using GoalService.Entities;
using HedefTakip.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Services;

public interface IGoalTemplateService
{
    Task<List<GoalTemplateDto>> GetAllAsync(bool onlyActive = true);
    Task<GoalTemplateDto?> GetByIdAsync(Guid id);
    Task<List<GoalTemplateDto>> GetByCategoryAsync(Guid categoryId);
    Task<GoalTemplateDto> CreateAsync(CreateGoalTemplateRequest request);
    Task<GoalTemplateDto?> UpdateAsync(Guid id, UpdateGoalTemplateRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public class GoalTemplateService : IGoalTemplateService
{
    private readonly GoalDbContext _db;

    public GoalTemplateService(GoalDbContext db) => _db = db;

    public async Task<List<GoalTemplateDto>> GetAllAsync(bool onlyActive = true)
    {
        var query = _db.GoalTemplates.Include(t => t.GoalCategory).AsQueryable();
        if (onlyActive)
            query = query.Where(t => t.IsActive);

        return await query
            .OrderBy(t => t.Title)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<GoalTemplateDto?> GetByIdAsync(Guid id)
    {
        var template = await _db.GoalTemplates
            .Include(t => t.GoalCategory)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        return template is null ? null : MapToDto(template);
    }

    public async Task<List<GoalTemplateDto>> GetByCategoryAsync(Guid categoryId)
    {
        return await _db.GoalTemplates
            .Include(t => t.GoalCategory)
            .Where(t => t.GoalCategoryId == categoryId && t.IsActive)
            .OrderBy(t => t.Title)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<GoalTemplateDto> CreateAsync(CreateGoalTemplateRequest request)
    {
        var categoryExists = await _db.GoalCategories.AnyAsync(c => c.Id == request.GoalCategoryId && c.IsActive);
        if (!categoryExists)
            throw new KeyNotFoundException(Messages.GoalCategory.NotFound);

        var template = new GoalTemplate
        {
            GoalCategoryId = request.GoalCategoryId,
            Title          = request.Title,
            Description    = request.Description,
            GoalType       = request.GoalType
        };

        _db.GoalTemplates.Add(template);
        await _db.SaveChangesAsync();

        await _db.Entry(template).Reference(t => t.GoalCategory).LoadAsync();
        return MapToDto(template);
    }

    public async Task<GoalTemplateDto?> UpdateAsync(Guid id, UpdateGoalTemplateRequest request)
    {
        var template = await _db.GoalTemplates
            .Include(t => t.GoalCategory)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        if (template is null) return null;

        var categoryExists = await _db.GoalCategories.AnyAsync(c => c.Id == request.GoalCategoryId && c.IsActive);
        if (!categoryExists)
            throw new KeyNotFoundException(Messages.GoalCategory.NotFound);

        template.GoalCategoryId = request.GoalCategoryId;
        template.Title          = request.Title;
        template.Description    = request.Description;
        template.GoalType       = request.GoalType;
        template.UpdatedTime    = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(template).Reference(t => t.GoalCategory).LoadAsync();
        return MapToDto(template);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var template = await _db.GoalTemplates.FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        if (template is null) return false;

        template.IsActive    = false;
        template.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static GoalTemplateDto MapToDto(GoalTemplate t) =>
        new(t.Id, t.GoalCategoryId, t.GoalCategory.Name, t.Title, t.Description, t.GoalType, t.IsActive, t.CreatedTime);
}
