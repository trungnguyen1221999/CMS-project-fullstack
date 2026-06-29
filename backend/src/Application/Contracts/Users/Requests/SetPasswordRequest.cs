using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Contracts.Users.Requests
{
    public class SetPasswordRequest
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
