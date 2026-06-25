using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Domain.Cores.Identity;

namespace Application.DTOs.Request
{
    public class CreateUserRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        public DateTime? Dob { get; set; }

        [MaxLength(500)]
        public string? Avatar { get; set; }
        public bool IsActive { get; set; }
        public decimal RoyaltyAmountPerPost { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<CreateUserRequestDto, User>();
            }
        }
    }
}
