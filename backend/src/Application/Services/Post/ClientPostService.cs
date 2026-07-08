using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using Microsoft.EntityFrameworkCore;
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;

namespace Application.Services.Post
{
    public class ClientPostService : IClientPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClientPostService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PageResult<PostInListResponse>> GetAllPostsAsync(PagingRequest request)
        {
            return await _unitOfWork.Posts.GetPublishedPostsAsync(request);
        }

        public async Task<PostResponse> GetPostByIdAsync(Guid postId)
        {
            var post =
                await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync()
                ?? throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            if (post.Status != Domain.Cores.Content.PostStatus.Published)
                throw new NotFoundException(ErrorMessages.Post.PostNotFound);

            return _mapper.Map<AppPost, PostResponse>(post);
        }

        public async Task<PageResult<PostInListResponse>> GetPostsByCategoryAsync(
            string categorySlug,
            PagingRequest request
        )
        {
            return await _unitOfWork.Posts.GetPostsByCategoryAsync(categorySlug, request);
        }

        public async Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PagingRequest request
        )
        {
            return await _unitOfWork.Posts.GetPostsByTagAsync(tagSlug, request);
        }
    }
}
