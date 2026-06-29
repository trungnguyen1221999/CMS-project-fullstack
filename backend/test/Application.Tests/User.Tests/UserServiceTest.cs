using Application.Repositories;
using Application.Services;
using AutoMapper;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IUserService _userService;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userManagerMock = MockUserManager.Create();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(
                _userRepositoryMock.Object,
                _userManagerMock.Object,
                _mapperMock.Object
            );
        }
    }
}
