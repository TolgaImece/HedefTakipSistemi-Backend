namespace IdentityService.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? PositionId,
    string? PositionName,
    bool IsActive
);

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? DepartmentId,
    Guid? PositionId
);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid? DepartmentId,
    Guid? PositionId,
    bool IsActive
);

public record ChangePasswordRequest(string NewPassword);

public record AssignDepartmentRequest(Guid? DepartmentId);
