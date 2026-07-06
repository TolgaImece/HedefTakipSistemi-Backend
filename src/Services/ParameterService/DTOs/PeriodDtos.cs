namespace ParameterService.DTOs;

public record PeriodDto(
    Guid Id,
    string Name,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsClosed,
    bool IsActive,
    bool IsEnabled,
    DateTime CreatedTime
);

public record CreatePeriodRequest(
    string Name,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsEnabled = true
);

public record UpdatePeriodRequest(
    string Name,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate
);
