using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Test.Shared.Helpers
{
    public static class AsyncQueryableExtensions
    {
        public static IQueryable<T> BuildMockQueryable<T>(this IEnumerable<T> source)
        {
            return new TestAsyncEnumerable<T>(source.AsQueryable());
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IQueryable<T> queryable)
            : base(queryable.Expression)
        {
            Provider = new TestAsyncQueryProvider<T>(queryable.Provider);
        }

        IQueryProvider IQueryable.Provider => Provider;
        public IQueryProvider Provider { get; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default
        )
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<T>(_inner.CreateQuery<T>(expression));

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(
                _inner.CreateQuery<TElement>(expression)
            );

        public object? Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) =>
            _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default
        )
        {
            var resultType = typeof(TResult).GetGenericArguments().FirstOrDefault();
            var executeMethod = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType!);

            var result = executeMethod.Invoke(_inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType!)
                .Invoke(null, [result])!;
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    }
}
