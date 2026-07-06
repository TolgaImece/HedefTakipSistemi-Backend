namespace IdentityService.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = UserRoles.User;

    // FK constraint yok — uygulama seviyesinde yönetilir
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }

    // Navigation — sadece DepartmentId/PositionId FK olduğunda çalışır
    public Department? Department { get; set; }
    public Position? Position { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] All = [Admin, Manager, User];
}
