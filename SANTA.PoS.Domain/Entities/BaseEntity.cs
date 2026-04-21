namespace SANTA.PoS.Domain.Entities;

/// <summary>
/// Base entity class - all domain entities should inherit from this
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
