using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequest request);
    Task<UserDto?> AssignDepartmentAsync(Guid userId, Guid? departmentId, Guid managerDeptId);
}

public class UserService : IUserService
{
    private readonly IdentityDbContext _db;

    public UserService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _db.Users
            .Include(u => u.Department)
            .Include(u => u.Position)
            .Where(u => u.IsActive)
            .Select(u => MapToDto(u))
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.Position)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");

        if (!UserRoles.All.Contains(request.Role))
            throw new InvalidOperationException($"Geçersiz rol: {request.Role}");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(user.Id) ?? MapToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return null;

        if (!UserRoles.All.Contains(request.Role))
            throw new InvalidOperationException($"Geçersiz rol: {request.Role}");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = request.Role;
        user.DepartmentId = request.DepartmentId;
        user.PositionId = request.PositionId;
        user.IsActive = request.IsActive;
        user.UpdatedTime = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return false;

        // Soft delete
        user.IsActive = false;
        user.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> AssignDepartmentAsync(Guid userId, Guid? departmentId, Guid managerDeptId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsActive) return null;

        // Çıkarma işleminde kullanıcı manager'ın departmanında olmalı
        if (!departmentId.HasValue && user.DepartmentId != managerDeptId) return null;

        user.DepartmentId = departmentId;
        if (!departmentId.HasValue) user.PositionId = null;
        user.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(userId);
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.DepartmentId,
        user.Department?.Name,
        user.PositionId,
        user.Position?.Name,
        user.IsActive
    );
}
