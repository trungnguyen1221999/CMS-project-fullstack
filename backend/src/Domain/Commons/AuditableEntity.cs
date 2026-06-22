namespace Domain.Commons
{
    public abstract class AuditableEntity : EntityBase, IAuditableEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
