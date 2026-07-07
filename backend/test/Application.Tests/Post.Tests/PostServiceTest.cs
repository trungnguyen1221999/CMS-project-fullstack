using Application.Services.Permission;
using Application.Services.Post;
using Application.UnitOfWork;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly IAdminPostService _adminPostService;
        private readonly IClientPostService _clientPostService;

        public PostServiceTest()
        {
            _mockUserManager = MockUserManager.Create();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _adminPostService = new AdminPostService(
                _mockUserManager.Object,
                _mockPermissionService.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object
            );
            _clientPostService = new ClientPostService(
                _mockUnitOfWork.Object,
                _mockMapper.Object
            );
        }
    }
}
