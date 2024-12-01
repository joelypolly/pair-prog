namespace PairProgrammingApi.DataAccess;

/// <summary>
///     Base class for all entities.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    ///     The unique ID of the entity.  This is picked up by convention.
    /// </summary>
    public virtual Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     The UTC date and time that the entity was created.
    /// </summary>
    public virtual DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    ///     A string which indicates the source that created the entity.
    /// </summary>
    public virtual string CreatedContext { get; set; } = "";

    /// <summary>
    ///     The UTC date and time that the entity was updated.
    /// </summary>
    public virtual DateTimeOffset? LastUpdatedUtc { get; set; }

    /// <summary>
    ///     A string which indicates the source of the last update on the entity.
    /// </summary>
    public virtual string? LastUpdateContext { get; set; }
}