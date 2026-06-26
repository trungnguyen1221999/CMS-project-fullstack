using System.ComponentModel.DataAnnotations;

namespace Application.Contracts.Users.Requests
{
    public class ChangeMyPasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(
            "NewPassword",
            ErrorMessage = "The new password and confirmation password do not match."
        )]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
