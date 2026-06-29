using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Domain.Cores.Identity;

namespace Application.Contracts.Users.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime? Dob { get; set; }

        [MaxLength(500)]
        public string? Avatar { get; set; }

        public bool IsActive { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal RoyaltyAmountPerPost { get; set; }

        public class AutoMapperProfiles : Profile
        {
            public AutoMapperProfiles()
            {
                CreateMap<UpdateUserRequest, User>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.UserName, opt => opt.Ignore())
                    .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                    .ForMember(dest => dest.Email, opt => opt.Ignore())
                    .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                    .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                    .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                    .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                    .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                    .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                    .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                    .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                    .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                    .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                    .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            }
        }
    }
}
