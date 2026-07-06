using HedefTakip.Shared.Constants;
using HedefTakip.Shared.Models;
using HedefTakip.Shared.Services;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IdentityDbContext _db;
    private readonly IAuditClient _audit;

    public DepartmentsController(IdentityDbContext db, IAuditClient audit)
    {
        _db = db;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _db.Departments
            .Include(d => d.Positions.Where(p => p.IsActive))
            .Where(d => d.IsActive)
            .Select(d => new DepartmentDto(
                d.Id, d.Name, d.IsActive,
                d.Positions.Select(p => new PositionDto(p.Id, p.DepartmentId, p.Name, p.IsActive)).ToList()
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<DepartmentDto>>.Ok(departments));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var d = await _db.Departments
            .Include(d => d.Positions.Where(p => p.IsActive))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (d is null)
            return NotFound(ApiResponse.Fail(Messages.Department.NotFound, Messages.Department.NotFoundUser, Messages.Department.NotFoundUser));

        return Ok(ApiResponse<DepartmentDto>.Ok(new(
            d.Id, d.Name, d.IsActive,
            d.Positions.Select(p => new PositionDto(p.Id, p.DepartmentId, p.Name, p.IsActive)).ToList()
        )));
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        if (await _db.Departments.AnyAsync(d => d.Name == request.Name))
            return BadRequest(ApiResponse.Fail(Messages.Department.AlreadyExists, Messages.Department.AlreadyExistsUser, Messages.Department.AlreadyExistsUser));

        var dept = new Department { Name = request.Name };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();

        _audit.Send("IdentityService", "CreateDepartment", "Department", dept.Id.ToString(), CurrentUserId, dept.Name);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id },
            ApiResponse<DepartmentDto>.Ok(new(dept.Id, dept.Name, dept.IsActive, []), Messages.Department.Created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept is null)
            return NotFound(ApiResponse.Fail(Messages.Department.NotFound, Messages.Department.NotFoundUser, Messages.Department.NotFoundUser));

        dept.Name = request.Name;
        dept.IsActive = request.IsActive;
        dept.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _audit.Send("IdentityService", "UpdateDepartment", "Department", id.ToString(), CurrentUserId, dept.Name);
        return Ok(ApiResponse<DepartmentDto>.Ok(new(dept.Id, dept.Name, dept.IsActive, []), Messages.Department.Updated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept is null)
            return NotFound(ApiResponse.Fail(Messages.Department.NotFound, Messages.Department.NotFoundUser, Messages.Department.NotFoundUser));

        dept.IsActive = false;
        dept.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _audit.Send("IdentityService", "DeleteDepartment", "Department", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.Department.Deleted));
    }

    [HttpGet("{departmentId:guid}/positions")]
    public async Task<IActionResult> GetPositions(Guid departmentId)
    {
        var positions = await _db.Positions
            .Where(p => p.DepartmentId == departmentId && p.IsActive)
            .Select(p => new PositionDto(p.Id, p.DepartmentId, p.Name, p.IsActive))
            .ToListAsync();

        return Ok(ApiResponse<List<PositionDto>>.Ok(positions));
    }

    [HttpPost("{departmentId:guid}/positions")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> CreatePosition(Guid departmentId, [FromBody] CreatePositionRequest request)
    {
        if (!await _db.Departments.AnyAsync(d => d.Id == departmentId))
            return NotFound(ApiResponse.Fail(Messages.Department.NotFound, Messages.Department.NotFoundUser, Messages.Department.NotFoundUser));

        var position = new Position { DepartmentId = departmentId, Name = request.Name };
        _db.Positions.Add(position);
        await _db.SaveChangesAsync();

        _audit.Send("IdentityService", "CreatePosition", "Position", position.Id.ToString(), CurrentUserId, position.Name);
        return CreatedAtAction(nameof(GetPositions), new { departmentId },
            ApiResponse<PositionDto>.Ok(new(position.Id, position.DepartmentId, position.Name, position.IsActive), Messages.Position.Created));
    }

    [HttpPut("positions/{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionRequest request)
    {
        var position = await _db.Positions.FindAsync(id);
        if (position is null)
            return NotFound(ApiResponse.Fail(Messages.Position.NotFound, Messages.Position.NotFoundUser, Messages.Position.NotFoundUser));

        position.Name = request.Name;
        position.IsActive = request.IsActive;
        position.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _audit.Send("IdentityService", "UpdatePosition", "Position", id.ToString(), CurrentUserId, position.Name);
        return Ok(ApiResponse<PositionDto>.Ok(new(position.Id, position.DepartmentId, position.Name, position.IsActive), Messages.Position.Updated));
    }

    [HttpDelete("positions/{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> DeletePosition(Guid id)
    {
        var position = await _db.Positions.FindAsync(id);
        if (position is null)
            return NotFound(ApiResponse.Fail(Messages.Position.NotFound, Messages.Position.NotFoundUser, Messages.Position.NotFoundUser));

        position.IsActive = false;
        position.UpdatedTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _audit.Send("IdentityService", "DeletePosition", "Position", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.Position.Deleted));
    }
}
