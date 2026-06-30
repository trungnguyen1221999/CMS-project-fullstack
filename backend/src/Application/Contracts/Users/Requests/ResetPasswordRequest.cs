using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Users.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Code must be exactly 4 digits.")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmationPassword { get; set; } = string.Empty;
    }
}
