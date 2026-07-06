namespace IdentityService.DTOs;

public record DepartmentDto(
    Guid Id,
    string Name,
    bool IsActive,
    List<PositionDto> Positions
);

public record CreateDepartmentRequest(string Name);

public record UpdateDepartmentRequest(string Name, bool IsActive);

public record PositionDto(
    Guid Id,
    Guid DepartmentId,
    string Name,
    bool IsActive
);

public record CreatePositionRequest(Guid DepartmentId, string Name);

public record UpdatePositionRequest(string Name, bool IsActive);
