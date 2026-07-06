using HedefTakip.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using ParameterService.Data;
using ParameterService.DTOs;
using ParameterService.Entities;

namespace ParameterService.Services;

public interface IPeriodService
{
    Task<List<PeriodDto>> GetAllAsync(bool onlyActive = true);
    Task<PeriodDto?> GetByIdAsync(Guid id);
    Task<PeriodDto> CreateAsync(CreatePeriodRequest request);
    Task<PeriodDto?> UpdateAsync(Guid id, UpdatePeriodRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<PeriodDto?> CloseAsync(Guid id);
    Task<PeriodDto?> SetEnabledAsync(Guid id, bool enabled);
}

public class PeriodService : IPeriodService
{
    private readonly ParameterDbContext _db;

    public PeriodService(ParameterDbContext db)
    {
        _db = db;
    }

    public async Task<List<PeriodDto>> GetAllAsync(bool onlyActive = true)
    {
        var query = _db.Periods.AsQueryable();
        if (onlyActive)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderByDescending(p => p.StartDate)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PeriodDto?> GetByIdAsync(Guid id)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        return period is null ? null : MapToDto(period);
    }

    public async Task<PeriodDto> CreateAsync(CreatePeriodRequest request)
    {
        if (await _db.Periods.AnyAsync(p => p.Name == request.Name))
            throw new InvalidOperationException(Messages.Period.AlreadyExists);

        var period = new Period
        {
            Name = request.Name,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsEnabled = request.IsEnabled
        };

        _db.Periods.Add(period);
        await _db.SaveChangesAsync();
        return MapToDto(period);
    }

    public async Task<PeriodDto?> UpdateAsync(Guid id, UpdatePeriodRequest request)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (period is null) return null;

        period.Name = request.Name;
        period.Type = request.Type;
        period.StartDate = request.StartDate;
        period.EndDate = request.EndDate;
        period.UpdatedTime = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(period);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (period is null) return false;

        period.IsActive = false;
        period.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PeriodDto?> CloseAsync(Guid id)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (period is null) return null;

        if (period.IsClosed)
            throw new InvalidOperationException(Messages.Period.AlreadyClosed);

        period.IsClosed = true;
        period.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(period);
    }

    public async Task<PeriodDto?> SetEnabledAsync(Guid id, bool enabled)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (period is null || period.IsClosed) return null;

        period.IsEnabled = enabled;
        period.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(period);
    }

    private static PeriodDto MapToDto(Period p) => new(
        p.Id, p.Name, p.Type, p.StartDate, p.EndDate, p.IsClosed, p.IsActive, p.IsEnabled, p.CreatedTime
    );
}
