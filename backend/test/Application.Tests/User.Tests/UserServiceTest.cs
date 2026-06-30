using Application.Repositories;
using Application.Services.User;
using Application.UnitOfWork;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IUserService _userService;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
            _userManagerMock = MockUserManager.Create();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(
                _userManagerMock.Object,
                _mapperMock.Object,
                _unitOfWorkMock.Object
            );
        }
    }
}
