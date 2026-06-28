using System;
using System.Collections.Generic;
using System.Text;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.Tests.Mocks
{
    public class MockUserManager
    {
        public static Mock<UserManager<User>> Create()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );
        }
    }
}
