using HedefTakip.Shared.Constants;
using HedefTakip.Shared.Models;
using HedefTakip.Shared.Services;
using IdentityService.DTOs;
using IdentityService.Entities;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditClient _audit;

    public UsersController(IUserService userService, IAuditClient audit)
    {
        _userService = userService;
        _audit = audit;
    }

    private string? CurrentUserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Manager}")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(ApiResponse<List<UserDto>>.Ok(users));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var currentUserId = CurrentUserId;
        var currentRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (currentRole != UserRoles.Admin && currentRole != UserRoles.Manager && currentUserId != id.ToString())
            return StatusCode(403, ApiResponse.Fail(Messages.Auth.Forbidden, Messages.Auth.ForbiddenUser, Messages.Auth.ForbiddenUser));

        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse.Fail(Messages.User.NotFound, Messages.User.NotFoundUser, Messages.User.NotFoundUser));

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);
            _audit.Send("IdentityService", "Create", "User", user.Id.ToString(),
                CurrentUserId, $"Email: {user.Email}, Rol: {user.Role}");
            return CreatedAtAction(nameof(GetById), new { id = user.Id },
                ApiResponse<UserDto>.Ok(user, Messages.User.Created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(Messages.User.AlreadyExists, ex.Message, Messages.User.AlreadyExistsUser));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateAsync(id, request);
            if (user is null)
                return NotFound(ApiResponse.Fail(Messages.User.NotFound, Messages.User.NotFoundUser, Messages.User.NotFoundUser));

            _audit.Send("IdentityService", "Update", "User", id.ToString(),
                CurrentUserId, $"Email: {user.Email}, Rol: {user.Role}");
            return Ok(ApiResponse<UserDto>.Ok(user, Messages.User.Updated));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(Messages.User.InvalidRole, ex.Message, Messages.User.InvalidRoleUser));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse.Fail(Messages.User.NotFound, Messages.User.NotFoundUser, Messages.User.NotFoundUser));

        _audit.Send("IdentityService", "Delete", "User", id.ToString(), CurrentUserId);
        return Ok(ApiResponse.OkNoData(Messages.User.Deleted));
    }

    [HttpPatch("{id:guid}/department")]
    [Authorize(Roles = UserRoles.Manager)]
    public async Task<IActionResult> AssignDepartment(Guid id, [FromBody] AssignDepartmentRequest request)
    {
        var managerDeptIdStr = User.FindFirst("departmentId")?.Value;
        if (string.IsNullOrEmpty(managerDeptIdStr) || !Guid.TryParse(managerDeptIdStr, out var managerDeptId))
            return StatusCode(403, ApiResponse.Fail(Messages.Auth.Forbidden, Messages.Auth.ForbiddenUser, Messages.Auth.ForbiddenUser));

        if (request.DepartmentId.HasValue && request.DepartmentId != managerDeptId)
            return StatusCode(403, ApiResponse.Fail(Messages.Auth.Forbidden, Messages.Auth.ForbiddenUser, Messages.Auth.ForbiddenUser));

        var result = await _userService.AssignDepartmentAsync(id, request.DepartmentId, managerDeptId);
        if (result is null)
            return NotFound(ApiResponse.Fail(Messages.User.NotFound, Messages.User.NotFoundUser, Messages.User.NotFoundUser));

        var auditDetails = request.DepartmentId.HasValue
            ? $"Departmana eklendi. DepartmentId: {managerDeptId}"
            : $"Departmandan çıkarıldı. DepartmentId: {managerDeptId}";
        _audit.Send("IdentityService", "Update", "User", id.ToString(), CurrentUserId, auditDetails);

        var msg = request.DepartmentId.HasValue ? "Kullanıcı departmana eklendi." : "Kullanıcı departmandan çıkarıldı.";
        return Ok(ApiResponse<UserDto>.Ok(result, msg));
    }

    [HttpPut("{id:guid}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        var currentUserId = CurrentUserId;

        if (currentUserId != id.ToString())
            return StatusCode(403, ApiResponse.Fail(Messages.Auth.Forbidden, Messages.Auth.ForbiddenUser, Messages.Auth.ForbiddenUser));

        var result = await _userService.ChangePasswordAsync(id, request);
        if (!result)
            return BadRequest(ApiResponse.Fail(Messages.Auth.WrongPassword, Messages.Auth.WrongPasswordUser, Messages.Auth.WrongPasswordUser));

        _audit.Send("IdentityService", "ChangePassword", "User", id.ToString(), currentUserId);
        return Ok(ApiResponse.OkNoData(Messages.Auth.PasswordChanged));
    }
}
