using Domain.Cores.Identity;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories
{
    public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context) { }

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
