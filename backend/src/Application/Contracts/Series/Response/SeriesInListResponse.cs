using System;
using System.Collections.Generic;
using System.Text;
using Domain.Commons;
using Domain.Cores.Content;

namespace Application.Contracts.Series.Response
{
    public class SeriesInListResponse : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string? SeoKeywords { get; set; }
        public Guid AuthorUserId { get; set; }

        public class AutoMapperProfile : AutoMapper.Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Serie, SeriesInListResponse>();
            }
        }
    }
}
