namespace IdentityService.Entities;

public class Position
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }

    public Department Department { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}
