using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Commons;

namespace BlogProject.Core.Domain.Content
{
    [Table("PostActivityLogs")]
    public class PostActivityLog : AuditableEntity
    {
        public Guid PostId { get; set; }

        public PostStatus FromStatus { set; get; }

        public PostStatus ToStatus { set; get; }

        [MaxLength(500)]
        public string? Note { set; get; }

        public Guid UserId { get; set; }

        [MaxLength(250)]
        public string? UserName { get; set; }
    }
}
