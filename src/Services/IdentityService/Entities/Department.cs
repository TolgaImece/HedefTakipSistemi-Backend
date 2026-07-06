namespace IdentityService.Entities;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }

    public ICollection<Position> Positions { get; set; } = new List<Position>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
