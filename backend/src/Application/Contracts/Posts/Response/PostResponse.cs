using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Domain.Cores.Content;

namespace Application.Contracts.Posts.Response
{
    public class PostResponse
    {
        public Guid CategoryId { get; set; }

        public string? Content { get; set; }

        public Guid AuthorUserId { get; set; }

        public string? Source { get; set; }

        public string? Tags { get; set; }

        public string? SeoDescription { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<Post, PostResponse>();
            }
        }
    }
}
