namespace MCP.Domain.Entities;

/// <summary>
///     Base class for all domain entities
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other || GetType() != other.GetType())
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }
}
