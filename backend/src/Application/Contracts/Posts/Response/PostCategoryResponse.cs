using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using AutoMapper;
using Domain.Commons;

namespace Application.Contracts.Posts.Response
{
    public class PostCategoryResponse : AuditableEntity
    {
        public string Name { set; get; } = string.Empty;

        public string Slug { set; get; } = string.Empty;

        public Guid? ParentId { set; get; }
        public bool IsActive { set; get; }

        public string? SeoDescription { set; get; }

        public int SortOrder { set; get; }

        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<PostResponse, PostCategoryResponse>();
            }
        }
    }
}
