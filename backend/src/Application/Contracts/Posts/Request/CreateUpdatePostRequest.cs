using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Domain.Cores.Content;

namespace Application.Contracts.Posts.Request
{
    public class CreateUpdatePostRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Thumbnail { get; set; }
        public Guid CategoryId { get; set; }

        public string? Content { get; set; }

        public string? Source { get; set; }

        public string[] Tags { get; set; } = new string[0];

        public string? SeoDescription { get; set; }

        public class AutoMapperProfiles : AutoMapper.Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<CreateUpdatePostRequest, Post>();
            }
        }
    }
}
