using System;
using System.Collections.Generic;
using System.Text;
using Application.Repositories;
using Domain.Cores.Content;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : RepositoryBase<PostCategory, Guid>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context)
            : base(context) { }
    }
}
