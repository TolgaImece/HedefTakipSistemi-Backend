namespace ParameterService.Entities;

public class Parameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Category { get; set; } = "System";
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedTime { get; set; }
}
