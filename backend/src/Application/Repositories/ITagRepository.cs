using System;
using System.Collections.Generic;
using System.Text;
using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface ITagRepository : IRepository<Tag, Guid> { }
}
