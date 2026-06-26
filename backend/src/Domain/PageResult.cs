namespace Domain
{
    public class PageResult<T> : PagedResultBase
    {
        public List<T> Result { get; set; }

        public PageResult()
        {
            Result = new List<T>();
        }
    }
}
