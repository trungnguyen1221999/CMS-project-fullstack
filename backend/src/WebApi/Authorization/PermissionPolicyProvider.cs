using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace WebApi.Authorization
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private const string PolicyPrefix = "Permissions.";
        private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(policyName))
                    .Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallbackProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            _fallbackProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            _fallbackProvider.GetFallbackPolicyAsync();
    }
}
