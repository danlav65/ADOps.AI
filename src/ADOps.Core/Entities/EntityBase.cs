namespace ADOps.Core.Entities;

/// <summary>
/// Base class for all domain entities.
/// Every entity is uniquely identifiable and tracked in UTC.
/// </summary>
public abstract class EntityBase
{
    protected EntityBase()
    {
        Id = Guid.NewGuid();
        CreatedUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedUtc { get; init; }

    /// <summary>
    /// UTC timestamp of the last modification.
    /// </summary>
    public DateTimeOffset? UpdatedUtc { get; private set; }

    /// <summary>
    /// Updates the modification timestamp.
    /// </summary>
    protected void Touch()
    {
        UpdatedUtc = DateTimeOffset.UtcNow;
    }
}