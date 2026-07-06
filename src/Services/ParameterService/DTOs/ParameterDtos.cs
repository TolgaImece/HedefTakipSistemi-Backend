namespace ParameterService.DTOs;

public record ParameterDto(
    Guid Id,
    string Category,
    string Key,
    string Value,
    string? Description,
    bool IsActive,
    DateTime CreatedTime
);

public record ParameterValueDto(string Key, string Value);

public record CreateParameterRequest(
    string Category,
    string Key,
    string Value,
    string? Description
);

public record UpdateParameterRequest(
    string Category,
    string Value,
    string? Description
);
