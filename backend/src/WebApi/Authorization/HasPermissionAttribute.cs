using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
            : base(policy: permission)
        {
        }
    }
}
