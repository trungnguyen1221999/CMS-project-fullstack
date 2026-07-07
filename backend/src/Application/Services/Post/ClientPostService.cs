using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ReadResponse<PageResult<PostInListResponse>>> GetAllPostsAsync(
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPublishedPostsAsync(request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<PostResponse>> GetPostByIdAsync(Guid postId)
        {
            var post = await _unitOfWork.Posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null || post.Status != Domain.Cores.Content.PostStatus.Published)
                return ReadResponse<PostResponse>.Failure(ErrorMessages.Post.PostNotFound);
            var postResponse = _mapper.Map<AppPost, PostResponse>(post);
            return ReadResponse<PostResponse>.Success(postResponse);
        }

        public async Task<
            ReadResponse<PageResult<PostInListResponse>>
        > GetPostsByCategoryAsync(string categorySlug, PostPagingRequest request)
        {
            var posts = await _unitOfWork.Posts.GetPostsByCategoryAsync(categorySlug, request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }

        public async Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        )
        {
            var posts = await _unitOfWork.Posts.GetPostsByTagAsync(tagSlug, request);
            return ReadResponse<PageResult<PostInListResponse>>.Success(posts);
        }
    }
}
