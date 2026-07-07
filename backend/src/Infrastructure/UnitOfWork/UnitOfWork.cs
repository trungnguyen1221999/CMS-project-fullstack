using Application.Repositories;
using AutoMapper;
using Infrastructure;
using Infrastructure.Repositories;

namespace Application.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserRepository Users { get; }
        public IPostRepository Posts { get; }
        public ICategoryRepository Categories { get; }
        public ITagRepository Tags { get; }
        public IPostTagsRepository PostTags { get; }

        public IPostInSeriesRepository PostInSeries { get; }
        private readonly IMapper _mapper;

        public UnitOfWork(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            Users = new UserRepository(context);
            Posts = new PostRepository(context, _mapper);
            Categories = new CategoryRepository(context);
            Tags = new TagRepository(context);
            PostTags = new PostTagsRepository(context);
            PostInSeries = new PostInSeriesRepository(context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
