using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Users.Requests
{
    public class ChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
