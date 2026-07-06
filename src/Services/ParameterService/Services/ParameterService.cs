using HedefTakip.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using ParameterService.Data;
using ParameterService.DTOs;

namespace ParameterService.Services;

public interface IParameterService
{
    Task<List<ParameterDto>> GetAllAsync(string? category = null);
    Task<ParameterDto?> GetByIdAsync(Guid id);
    Task<ParameterValueDto?> GetByKeyAsync(string key);
    Task<List<ParameterValueDto>> GetByKeysAsync(IEnumerable<string> keys);
    Task<ParameterDto> CreateAsync(CreateParameterRequest request);
    Task<ParameterDto?> UpdateAsync(Guid id, UpdateParameterRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public class ParameterLookupService : IParameterService
{
    private readonly ParameterDbContext _db;

    public ParameterLookupService(ParameterDbContext db)
    {
        _db = db;
    }

    public async Task<List<ParameterDto>> GetAllAsync(string? category = null)
    {
        var query = _db.Parameters.Where(p => p.IsActive);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        return await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Key)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<ParameterDto?> GetByIdAsync(Guid id)
    {
        var p = await _db.Parameters.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        return p is null ? null : MapToDto(p);
    }

    public async Task<ParameterValueDto?> GetByKeyAsync(string key)
    {
        var p = await _db.Parameters.FirstOrDefaultAsync(p => p.Key == key && p.IsActive);
        return p is null ? null : new ParameterValueDto(p.Key, p.Value);
    }

    public async Task<List<ParameterValueDto>> GetByKeysAsync(IEnumerable<string> keys)
    {
        var keyList = keys.ToList();
        return await _db.Parameters
            .Where(p => keyList.Contains(p.Key) && p.IsActive)
            .Select(p => new ParameterValueDto(p.Key, p.Value))
            .ToListAsync();
    }

    public async Task<ParameterDto> CreateAsync(CreateParameterRequest request)
    {
        if (await _db.Parameters.AnyAsync(p => p.Key == request.Key))
            throw new InvalidOperationException(Messages.General.ValidationError);

        var parameter = new Entities.Parameter
        {
            Category = request.Category,
            Key = request.Key,
            Value = request.Value,
            Description = request.Description
        };

        _db.Parameters.Add(parameter);
        await _db.SaveChangesAsync();
        return MapToDto(parameter);
    }

    public async Task<ParameterDto?> UpdateAsync(Guid id, UpdateParameterRequest request)
    {
        var parameter = await _db.Parameters.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (parameter is null) return null;

        parameter.Category = request.Category;
        parameter.Value = request.Value;
        parameter.Description = request.Description;
        parameter.UpdatedTime = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(parameter);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var parameter = await _db.Parameters.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (parameter is null) return false;

        parameter.IsActive = false;
        parameter.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private static ParameterDto MapToDto(Entities.Parameter p) => new(
        p.Id, p.Category, p.Key, p.Value, p.Description, p.IsActive, p.CreatedTime
    );
}
