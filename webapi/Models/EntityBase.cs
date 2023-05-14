namespace webapi.Models;

public abstract class EntityBase
{
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();
    public virtual DateTimeOffset? CreatedOn { get; set; }
    public virtual string? CreatedBy { get; set; }
    public virtual DateTimeOffset? UpdatedOn { get; set; }
    public virtual string? UpdatedBy { get; set; }
    public virtual DateTimeOffset? DeletedOn { get; set; }
    public virtual string? DeletedBy { get; set; }
    public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public List<IEvent> Events { get; set; } = new();
}
