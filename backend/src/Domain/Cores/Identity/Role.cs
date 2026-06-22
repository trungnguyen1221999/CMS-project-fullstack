using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BlogProject.Core.Domain.Identity
{
    [Table("Roles")]
    public class Role : IdentityRole<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
