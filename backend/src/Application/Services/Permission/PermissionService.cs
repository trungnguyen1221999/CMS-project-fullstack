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

        public bool HasDeletePostPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.Posts.Delete);
        }

        public bool HasViewActiveCategoryPermission(Guid userId, bool isPostActive)
        {
            var permissions = GetPermissionsByUserId(userId);
            return (permissions.Contains(Permissions.PostCategories.View) && isPostActive);
        }

        public bool HasViewAllCategoriesPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.PostCategories.View);
        }

        public bool HasViewAllCategoryPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.PostCategories.Edit);
        }

        public bool HasCreateCategoryPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.PostCategories.Create);
        }

        public bool HasEditCategoryPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.PostCategories.Edit);
        }

        public bool HasDeleteCategoryPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.PostCategories.Delete);
        }

        public bool HasRoyaltyReportViewPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.Royalty.View);
        }

        public bool HasRoyaltyPayPermission(Guid userId)
        {
            var permissions = GetPermissionsByUserId(userId);
            return permissions.Contains(Permissions.Royalty.Pay);
        }
    }
}
