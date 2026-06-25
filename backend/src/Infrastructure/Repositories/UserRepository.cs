using Application.DTOs;
using Domain.Cores.Identity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories
{
    public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<UserDto?> GetByIdWithRolesAsync(Guid userId)
        {
            var userWithRoles = await (
                from user in _context.Users.AsNoTracking()
                where user.Id == userId
                join userRole in _context.UserRoles.AsNoTracking()
                    on user.Id equals userRole.UserId into userRoles
                from userRole in userRoles.DefaultIfEmpty()
                join role in _context.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id into roles
                from role in roles.DefaultIfEmpty()
                select new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.CreatedAt,
                    user.IsActive,
                    user.Dob,
                    user.Avatar,
                    user.VipStartDate,
                    user.VipExpireDate,
                    user.LastLoginDate,
                    user.Balance,
                    user.RoyaltyAmountPerPost,
                    RoleName = role != null ? role.Name : null,
                }
            ).ToListAsync();

            if (!userWithRoles.Any())
                return null;

            var result = userWithRoles
                .GroupBy(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.UserName,
                    x.Email,
                    x.PhoneNumber,
                    x.CreatedAt,
                    x.IsActive,
                    x.Dob,
                    x.Avatar,
                    x.VipStartDate,
                    x.VipExpireDate,
                    x.LastLoginDate,
                    x.Balance,
                    x.RoyaltyAmountPerPost,
                })
                .Select(g => new UserDto
                {
                    Id = g.Key.Id,
                    FirstName = g.Key.FirstName,
                    LastName = g.Key.LastName,
                    UserName = g.Key.UserName ?? string.Empty,
                    Email = g.Key.Email ?? string.Empty,
                    PhoneNumber = g.Key.PhoneNumber ?? string.Empty,
                    CreatedAt = g.Key.CreatedAt,
                    IsActive = g.Key.IsActive,
                    Dob = g.Key.Dob,
                    Avatar = g.Key.Avatar,
                    VipStartDate = g.Key.VipStartDate,
                    VipExpireDate = g.Key.VipExpireDate,
                    LastLoginDate = g.Key.LastLoginDate,
                    Balance = g.Key.Balance,
                    RoyaltyAmountPerPost = g.Key.RoyaltyAmountPerPost,
                    Roles = g
                        .Where(x => !string.IsNullOrEmpty(x.RoleName))
                        .Select(x => x.RoleName!)
                        .Distinct()
                        .ToList(),
                })
                .FirstOrDefault();

            return result;
        }

        public async Task<IEnumerable<UserListItemDto>> GetAllWithRolesAsync()
        {
            var usersWithRoles = await (
                from user in _context.Users.AsNoTracking()
                join userRole in _context.UserRoles.AsNoTracking()
                    on user.Id equals userRole.UserId into userRoles
                from userRole in userRoles.DefaultIfEmpty()
                join role in _context.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id into roles
                from role in roles.DefaultIfEmpty()
                select new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.UserName,
                    user.Email,
                    user.CreatedAt,
                    user.IsActive,
                    RoleName = role != null ? role.Name : null,
                }
            ).ToListAsync();

            var result = usersWithRoles
                .GroupBy(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.UserName,
                    x.Email,
                    x.CreatedAt,
                    x.IsActive,
                })
                .Select(g => new UserListItemDto
                {
                    Id = g.Key.Id,
                    FirstName = g.Key.FirstName,
                    LastName = g.Key.LastName,
                    UserName = g.Key.UserName ?? string.Empty,
                    Email = g.Key.Email ?? string.Empty,
                    CreatedAt = g.Key.CreatedAt,
                    IsActive = g.Key.IsActive,
                    Roles = g
                        .Where(x => !string.IsNullOrEmpty(x.RoleName))
                        .Select(x => x.RoleName!)
                        .Distinct()
                        .ToList(),
                })
                .ToList();

            return result;
        }

        public async Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames)
        {
            if (!roleNames.Any())
                return;
            foreach (var roleName in roleNames)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    return;
                var userRole = await _context.UserRoles.FirstOrDefaultAsync(r =>
                    r.UserId == userId && r.RoleId == role.Id
                );
                if (userRole == null)
                    return;

                _context.UserRoles.Remove(userRole);
            }
        }
    }
}
