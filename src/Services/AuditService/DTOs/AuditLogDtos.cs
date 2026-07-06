namespace AuditService.DTOs;

public record AuditLogDto(
    Guid Id,
    string ServiceName,
    string Action,
    string? EntityName,
    string? EntityId,
    string? UserId,
    string? Details,
    string? IpAddress,
    DateTime CreatedTime
);

public record CreateAuditLogRequest(
    string ServiceName,
    string Action,
    string? EntityName,
    string? EntityId,
    string? UserId,
    string? Details
);

public record AuditLogQueryParams(
    string? ServiceName,
    string? Action,
    string? EntityName,
    string? UserId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50
);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
