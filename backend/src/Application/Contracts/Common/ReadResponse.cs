namespace Application.Contracts.Common
{
    public class ReadResponse<T> : WriteResponse
    {
        public T? Data { get; set; }
    }
}
