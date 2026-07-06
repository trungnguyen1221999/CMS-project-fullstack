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
                    .ForAllMembers(opt =>
                        opt.Condition((src, dest, srcMember) => srcMember != null)
                    );
            }
        }
    }
}
