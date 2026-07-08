using Application.Contracts.Common;
using Application.Contracts.Users.Responses;
using Application.Repositories;
using Domain;
using Domain.Cores.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<UserResponse?> GetByIdWithRolesAsync(Guid userId)
        {
            var userWithRoles = await (
                from user in _context.Users.AsNoTracking()
                where user.Id == userId
                join userRole in _context.UserRoles.AsNoTracking()
                    on user.Id equals userRole.UserId
                    into userRoles
                from userRole in userRoles.DefaultIfEmpty()
                join role in _context.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id
                    into roles
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
                .Select(g => new UserResponse
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
                    Roles = g.Where(x => !string.IsNullOrEmpty(x.RoleName))
                        .Select(x => x.RoleName!)
                        .Distinct()
                        .ToList(),
                })
                .FirstOrDefault();

            return result;
        }

        public async Task<PageResult<UserListItemResponse>> GetAllWithRolesAsync(
            PagingRequest request
        )
        {
            var currentPage = request.CurrentPage <= 0 ? 1 : request.CurrentPage;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var usersQuery = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                usersQuery = usersQuery.Where(x =>
                    x.FirstName.Contains(keyword)
                    || x.LastName.Contains(keyword)
                    || (x.Email != null && x.Email.Contains(keyword))
                    || (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword))
                );
            }

            var totalCount = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.UserName,
                    x.Email,
                    x.CreatedAt,
                    x.IsActive,
                })
                .ToListAsync();

            var userIds = users.Select(x => x.Id).ToList();
            var userRoles = await (
                from userRole in _context.UserRoles.AsNoTracking()
                join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, RoleName = role.Name }
            ).ToListAsync();

            var result = users
                .Select(user => new UserListItemResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    Roles = userRoles
                        .Where(x => x.UserId == user.Id && !string.IsNullOrEmpty(x.RoleName))
                        .Select(x => x.RoleName!)
                        .Distinct()
                        .ToList(),
                })
                .ToList();

            return new PageResult<UserListItemResponse>
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
                Result = result,
            };
        }

        public async Task<int> DeleteByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.Distinct().ToList();
            if (!idList.Any())
                return 0;

            return await _context.Users.Where(u => idList.Contains(u.Id)).ExecuteDeleteAsync();
        }

        public async Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames)
        {
            if (!roleNames.Any())
                return;
            foreach (var roleName in roleNames)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    continue;
                var userRole = await _context.UserRoles.FirstOrDefaultAsync(r =>
                    r.UserId == userId && r.RoleId == role.Id
                );
                if (userRole == null)
                    continue;

                _context.UserRoles.Remove(userRole);
            }
        }

    }
}
