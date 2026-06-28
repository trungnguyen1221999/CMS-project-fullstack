using Domain.Cores.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Test.Shared.Mocks
{
    public static class MockSignInManager
    {
        public static Mock<SignInManager<User>> Create(Mock<UserManager<User>>? userManagerMock = null)
        {
            userManagerMock ??= MockUserManager.Create();
            return new Mock<SignInManager<User>>(
                userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<User>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object
            );
        }
    }
}
