using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement
        )
        {
            var permissionsClaim = context.User.FindFirst(UserClaims.Permissions);
            if (permissionsClaim == null)
                return Task.CompletedTask;

            var permissions = permissionsClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (permissions.Contains(requirement.Permission))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
