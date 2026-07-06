using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly IPostService _postService;
        private readonly Mock<IMapper> _mockMapper;

        public PostServiceTest()
        {
            _mockUserManager = MockUserManager.Create();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _postService = new PostService(
                _mockUserManager.Object,
                _mockPermissionService.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object
            );
        }
    }
}
