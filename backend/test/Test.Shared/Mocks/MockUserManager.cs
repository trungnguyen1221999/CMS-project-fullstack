using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Test.Shared.Mocks
{
    public static class MockUserManager
    {
        public static Mock<UserManager<User>> Create()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}
