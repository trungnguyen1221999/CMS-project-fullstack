using Application.Repositories;
using Application.Services.Permission;
using Application.Services.Royalty;
using Application.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Royalty.Tests
{
    public partial class RoyaltyServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPostRepository> _mockPostRepository;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly IRoyaltyService _royaltyService;

        public RoyaltyServiceTest()
        {
            _mockPostRepository = new Mock<IPostRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.Posts).Returns(_mockPostRepository.Object);
            _mockPermissionService = new Mock<IPermissionService>();
            _mockUserManager = MockUserManager.Create();
            _royaltyService = new RoyaltyService(
                _mockPermissionService.Object,
                _mockUnitOfWork.Object,
                _mockUserManager.Object
            );
        }
    }
}
