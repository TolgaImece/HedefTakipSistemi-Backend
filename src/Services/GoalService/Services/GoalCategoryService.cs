using GoalService.Data;
using GoalService.DTOs;
using GoalService.Entities;
using HedefTakip.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Services;

public interface IGoalCategoryService
{
    Task<List<GoalCategoryDto>> GetAllAsync(bool onlyActive = true);
    Task<GoalCategoryDto?> GetByIdAsync(Guid id);
    Task<GoalCategoryDto> CreateAsync(CreateGoalCategoryRequest request);
    Task<GoalCategoryDto?> UpdateAsync(Guid id, UpdateGoalCategoryRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public class GoalCategoryService : IGoalCategoryService
{
    private readonly GoalDbContext _db;

    public GoalCategoryService(GoalDbContext db) => _db = db;

    public async Task<List<GoalCategoryDto>> GetAllAsync(bool onlyActive = true)
    {
        var query = _db.GoalCategories.AsQueryable();
        if (onlyActive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<GoalCategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _db.GoalCategories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        return category is null ? null : MapToDto(category);
    }

    public async Task<GoalCategoryDto> CreateAsync(CreateGoalCategoryRequest request)
    {
        if (await _db.GoalCategories.AnyAsync(c => c.Name == request.Name))
            throw new InvalidOperationException(Messages.GoalCategory.AlreadyExists);

        var category = new GoalCategory { Name = request.Name };
        _db.GoalCategories.Add(category);
        await _db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<GoalCategoryDto?> UpdateAsync(Guid id, UpdateGoalCategoryRequest request)
    {
        var category = await _db.GoalCategories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (category is null) return null;

        if (await _db.GoalCategories.AnyAsync(c => c.Name == request.Name && c.Id != id))
            throw new InvalidOperationException(Messages.GoalCategory.AlreadyExists);

        category.Name = request.Name;
        category.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _db.GoalCategories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (category is null) return false;

        category.IsActive = false;
        category.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static GoalCategoryDto MapToDto(GoalCategory c) =>
        new(c.Id, c.Name, c.IsActive, c.CreatedTime);
}
