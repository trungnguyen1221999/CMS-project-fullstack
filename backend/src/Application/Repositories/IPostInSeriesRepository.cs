using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface IPostInSeriesRepository : IRepository<PostInSeries, Guid>
    {
        public bool RemovePostFromSeries(Guid postId, Guid seriesId);
        public bool ClearPostFromAllSeries(Guid postId);
    }
}
