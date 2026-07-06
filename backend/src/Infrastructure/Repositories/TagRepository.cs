using System;
using System.Collections.Generic;
using System.Text;
using Application.Repositories;
using Domain.Cores.Content;

namespace Infrastructure.Repositories
{
    public class TagRepository : RepositoryBase<Tag, Guid>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context)
            : base(context) { }
    }
}
