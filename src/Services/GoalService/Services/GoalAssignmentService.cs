using GoalService.Data;
using GoalService.DTOs;
using GoalService.Entities;
using HedefTakip.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Services;

public interface IGoalAssignmentService
{
    Task<List<GoalAssignmentDto>> GetAllAsync();
    Task<List<GoalAssignmentDto>> GetByUserAsync(Guid userId);
    Task<List<GoalAssignmentDto>> GetByPeriodAsync(Guid periodId);
    Task<GoalAssignmentDto?> GetByIdAsync(Guid id);
    Task<GoalAssignmentDto> CreateAsync(CreateGoalAssignmentRequest request);
    Task<GoalAssignmentDto?> UpdateStatusAsync(Guid id, UpdateGoalAssignmentStatusRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public class GoalAssignmentService : IGoalAssignmentService
{
    private readonly GoalDbContext _db;

    public GoalAssignmentService(GoalDbContext db) => _db = db;

    public async Task<List<GoalAssignmentDto>> GetAllAsync()
    {
        return await _db.GoalAssignments
            .Include(a => a.GoalTemplate).ThenInclude(t => t.GoalCategory)
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<List<GoalAssignmentDto>> GetByUserAsync(Guid userId)
    {
        return await _db.GoalAssignments
            .Include(a => a.GoalTemplate).ThenInclude(t => t.GoalCategory)
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderByDescending(a => a.CreatedTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<List<GoalAssignmentDto>> GetByPeriodAsync(Guid periodId)
    {
        return await _db.GoalAssignments
            .Include(a => a.GoalTemplate).ThenInclude(t => t.GoalCategory)
            .Where(a => a.PeriodId == periodId && a.IsActive)
            .OrderByDescending(a => a.CreatedTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<GoalAssignmentDto?> GetByIdAsync(Guid id)
    {
        var assignment = await _db.GoalAssignments
            .Include(a => a.GoalTemplate).ThenInclude(t => t.GoalCategory)
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        return assignment is null ? null : MapToDto(assignment);
    }

    public async Task<GoalAssignmentDto> CreateAsync(CreateGoalAssignmentRequest request)
    {
        var templateExists = await _db.GoalTemplates.AnyAsync(t => t.Id == request.GoalTemplateId && t.IsActive);
        if (!templateExists)
            throw new KeyNotFoundException(Messages.GoalTemplate.NotFound);

        var assignment = new GoalAssignment
        {
            GoalTemplateId = request.GoalTemplateId,
            UserId         = request.UserId,
            PeriodId       = request.PeriodId
        };

        _db.GoalAssignments.Add(assignment);
        await _db.SaveChangesAsync();

        await _db.Entry(assignment).Reference(a => a.GoalTemplate).LoadAsync();
        await _db.Entry(assignment.GoalTemplate).Reference(t => t.GoalCategory).LoadAsync();
        return MapToDto(assignment);
    }

    public async Task<GoalAssignmentDto?> UpdateStatusAsync(Guid id, UpdateGoalAssignmentStatusRequest request)
    {
        if (!GoalAssignmentStatus.All.Contains(request.Status))
            throw new InvalidOperationException(Messages.GoalAssignment.InvalidStatus);

        var assignment = await _db.GoalAssignments
            .Include(a => a.GoalTemplate).ThenInclude(t => t.GoalCategory)
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        if (assignment is null) return null;

        assignment.Status      = request.Status;
        assignment.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(assignment);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assignment = await _db.GoalAssignments.FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        if (assignment is null) return false;

        assignment.IsActive    = false;
        assignment.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static GoalAssignmentDto MapToDto(GoalAssignment a) =>
        new(a.Id, a.GoalTemplateId, a.GoalTemplate.Title, a.GoalTemplate.GoalType,
            a.GoalTemplate.GoalCategory.Name, a.UserId, a.PeriodId, a.Status, a.IsActive, a.CreatedTime);
}
