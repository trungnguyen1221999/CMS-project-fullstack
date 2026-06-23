using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Commons;
using Microsoft.EntityFrameworkCore;

namespace Domain.Cores.Content
{
    [Table("Series")]
    [Index(nameof(Slug), IsUnique = true)]
    public class Serie : AuditableEntity
    {
        [MaxLength(250)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public int SortOrder { get; set; }

        [MaxLength(250)]
        public string? SeoDescription { get; set; }

        [MaxLength(250)]
        public string? Thumbnail { set; get; }

        public string? Content { get; set; }
        public Guid AuthorUserId { get; set; }
    }
}
