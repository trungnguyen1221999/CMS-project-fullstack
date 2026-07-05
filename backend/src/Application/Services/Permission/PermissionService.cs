using System.Security.Claims;
using Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Permission
{
    public class PermissionService : IPermissionService
    {
        private readonly ClaimsPrincipal _claimsPrincipal;

        public PermissionService(IHttpContextAccessor httpContextAccessor)
        {
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
        }

        public List<string> GetPermissionsByUserId(Guid userId)
        {
            var permissionClaim = _claimsPrincipal.FindFirst(UserClaims.Permissions);
            if (permissionClaim == null || string.IsNullOrEmpty(permissionClaim.Value))
                return [];

            return permissionClaim.Value.Split(',').ToList();
        }

        public bool HasApprovedPostPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.Posts.Approve);
        }
    }
}
