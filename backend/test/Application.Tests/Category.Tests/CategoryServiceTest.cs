using Application.Repositories;
using Application.Services.Category;
using Application.Services.Permission;
using Application.UnitOfWork;
using AutoMapper;
using Moq;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly ICategoryService _categoryService;

        public CategoryServiceTest()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.Categories).Returns(_mockCategoryRepository.Object);
            _mockMapper = new Mock<IMapper>();
            _mockPermissionService = new Mock<IPermissionService>();
            _categoryService = new CategoryService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockPermissionService.Object
            );
        }
    }
}
