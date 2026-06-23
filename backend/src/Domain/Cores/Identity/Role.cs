using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain.Cores.Identity
{
    public class Role : IdentityRole<Guid>
    {
        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
