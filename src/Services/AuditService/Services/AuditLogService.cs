using AuditService.Data;
using AuditService.DTOs;
using AuditService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Services;

public interface IAuditLogService
{
    Task<AuditLogDto> CreateAsync(CreateAuditLogRequest request, string? ipAddress);
    Task<PagedResult<AuditLogDto>> QueryAsync(AuditLogQueryParams queryParams);
    Task<AuditLogDto?> GetByIdAsync(Guid id);
}

public class AuditLogService : IAuditLogService
{
    private readonly AuditDbContext _db;

    public AuditLogService(AuditDbContext db) => _db = db;

    public async Task<AuditLogDto> CreateAsync(CreateAuditLogRequest request, string? ipAddress)
    {
        var log = new AuditLog
        {
            ServiceName = request.ServiceName,
            Action      = request.Action,
            EntityName  = request.EntityName,
            EntityId    = request.EntityId,
            UserId      = request.UserId,
            Details     = request.Details,
            IpAddress   = ipAddress
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
        return MapToDto(log);
    }

    public async Task<PagedResult<AuditLogDto>> QueryAsync(AuditLogQueryParams q)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(q.ServiceName))
            query = query.Where(l => l.ServiceName.Contains(q.ServiceName));

        if (!string.IsNullOrEmpty(q.Action))
            query = query.Where(l => l.Action.Contains(q.Action));

        if (!string.IsNullOrEmpty(q.EntityName))
            query = query.Where(l => l.EntityName != null && l.EntityName.Contains(q.EntityName));

        if (!string.IsNullOrEmpty(q.UserId))
            query = query.Where(l => l.UserId == q.UserId);

        if (q.From.HasValue)
            query = query.Where(l => l.CreatedTime >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(l => l.CreatedTime <= q.To.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedTime)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return new PagedResult<AuditLogDto>(items, totalCount, q.Page, q.PageSize);
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id)
    {
        var log = await _db.AuditLogs.FirstOrDefaultAsync(l => l.Id == id);
        return log is null ? null : MapToDto(log);
    }

    private static AuditLogDto MapToDto(AuditLog l) =>
        new(l.Id, l.ServiceName, l.Action, l.EntityName, l.EntityId, l.UserId, l.Details, l.IpAddress, l.CreatedTime);
}
