using System.ComponentModel.DataAnnotations;
namespace Domain.Commons
{
    public abstract class EntityBase : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
    }
}
