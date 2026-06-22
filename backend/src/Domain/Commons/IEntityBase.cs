namespace Domain.Commons
{
    public interface IEntityBase<T>
    {
        T Id { get; set; }
    }
}
