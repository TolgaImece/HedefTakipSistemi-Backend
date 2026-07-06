using GoalService.Data;
using GoalService.DTOs;
using GoalService.Entities;
using HedefTakip.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Services;

public interface IGoalCommentService
{
    Task<List<GoalCommentDto>> GetByAssignmentAsync(Guid assignmentId);
    Task<GoalCommentDto> CreateAsync(Guid assignmentId, Guid userId, CreateGoalCommentRequest request);
    Task<bool> DeleteAsync(Guid id, Guid requestingUserId, bool isAdmin);
}

public class GoalCommentService : IGoalCommentService
{
    private readonly GoalDbContext _db;

    public GoalCommentService(GoalDbContext db) => _db = db;

    public async Task<List<GoalCommentDto>> GetByAssignmentAsync(Guid assignmentId)
    {
        return await _db.GoalComments
            .Where(c => c.GoalAssignmentId == assignmentId)
            .OrderBy(c => c.CreatedTime)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<GoalCommentDto> CreateAsync(Guid assignmentId, Guid userId, CreateGoalCommentRequest request)
    {
        var assignmentExists = await _db.GoalAssignments.AnyAsync(a => a.Id == assignmentId && a.IsActive);
        if (!assignmentExists)
            throw new KeyNotFoundException(Messages.GoalAssignment.NotFound);

        var comment = new GoalComment
        {
            GoalAssignmentId = assignmentId,
            UserId           = userId,
            Content          = request.Content
        };

        _db.GoalComments.Add(comment);
        await _db.SaveChangesAsync();
        return MapToDto(comment);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid requestingUserId, bool isAdmin)
    {
        var comment = await _db.GoalComments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return false;

        // Sadece yorumun sahibi veya admin silebilir
        if (!isAdmin && comment.UserId != requestingUserId)
            throw new UnauthorizedAccessException(Messages.Auth.Forbidden);

        _db.GoalComments.Remove(comment);
        await _db.SaveChangesAsync();
        return true;
    }

    private static GoalCommentDto MapToDto(GoalComment c) =>
        new(c.Id, c.GoalAssignmentId, c.UserId, c.Content, c.CreatedTime);
}
