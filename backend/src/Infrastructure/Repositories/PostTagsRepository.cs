using System;
using System.Collections.Generic;
using System.Text;
using Application.Repositories;
using Domain.Cores.Content;

namespace Infrastructure.Repositories
{
    public class PostTagsRepository : RepositoryBase<PostTag, Guid>, IPostTagsRepository
    {
        public PostTagsRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<bool> AddTagToPostAsync(Guid postId, Guid tagId)
        {
            var postTag = new PostTag { PostId = postId, TagId = tagId };
            await _context.AddAsync(postTag);
            return true;
        }
    }
}
