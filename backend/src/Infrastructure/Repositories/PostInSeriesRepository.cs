using System;
using System.Collections.Generic;
using System.Text;
using Application.Repositories;
using Domain.Cores.Content;

namespace Infrastructure.Repositories
{
    public class PostInSeriesRepository
        : RepositoryBase<PostInSeries, Guid>,
            IPostInSeriesRepository
    {
        public PostInSeriesRepository(ApplicationDbContext context)
            : base(context) { }

        public bool ClearPostFromAllSeries(Guid postId)
        {
            var postInSeries = _context.PostInSeries.Where(ps => ps.PostId == postId).ToList();
            if (!postInSeries.Any())
                return false;
            _context.PostInSeries.RemoveRange(postInSeries);
            return true;
        }

        public bool RemovePostFromSeries(Guid postId, Guid seriesId)
        {
            var postInSeries = _context.PostInSeries.Find(postId, seriesId);
            if (postInSeries == null)
                return false;
            _context.PostInSeries.Remove(postInSeries);
            return true;
        }
    }
}
