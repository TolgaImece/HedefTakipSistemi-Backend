namespace AuditService.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ServiceName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}

public static class AuditAction
{
    public const string Create       = "Create";
    public const string Update       = "Update";
    public const string Delete       = "Delete";
    public const string Login        = "Login";
    public const string Logout       = "Logout";
    public const string StatusChange = "StatusChange";

    public static readonly string[] All = [Create, Update, Delete, Login, Logout, StatusChange];
}

public static class AuditServiceName
{
    public const string IdentityService  = "IdentityService";
    public const string ParameterService = "ParameterService";
    public const string GoalService      = "GoalService";

    public static readonly string[] All = [IdentityService, ParameterService, GoalService];
}
