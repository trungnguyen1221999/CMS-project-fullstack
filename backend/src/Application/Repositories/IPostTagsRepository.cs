using System;
using System.Collections.Generic;
using System.Text;
using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface IPostTagsRepository : IRepository<PostTag, Guid>
    {
        Task<bool> AddTagToPostAsync(Guid postId, Guid tagId);
    }
}
