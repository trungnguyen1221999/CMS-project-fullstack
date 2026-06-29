using Application.Constants;
using Microsoft.AspNetCore.Identity;
using Moq;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        private static AppUser CreateUserForAssignRoles(Guid? id = null) =>
            new() { Id = id ?? Guid.NewGuid(), Email = "test@test.com" };

        private void SetupGetRoles(AppUser user, IList<string> roles) =>
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

        private void SetupAddToRoles(AppUser user, string[] roles, IdentityResult result) =>
            _userManagerMock.Setup(x => x.AddToRolesAsync(user, roles)).ReturnsAsync(result);

        [Fact]
        public async Task AssignRolesToUserAsync_UserNotFound_ReturnFailure()
        {
            //Arrange
            var id = Guid.NewGuid();
            var roles = new[] { "Admin", "Editor" };
            SetupFindUser(id, null);

            //Act
            var result = await _userService.AssignRolesToUserAsync(id, roles);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
            _userManagerMock.Verify(
                x => x.AddToRolesAsync(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()),
                Times.Never
            );
        }

        [Fact]
        public async Task AssignRolesToUserAsync_AddToRolesFailed_ReturnFailure()
        {
            //Arrange
            var id = Guid.NewGuid();
            var user = CreateUserForAssignRoles(id);
            var roles = new[] { "Admin", "Editor" };
            var currentRoles = new List<string> { "User" };
            SetupFindUser(id, user);
            SetupGetRoles(user, currentRoles);
            _userRepositoryMock
                .Setup(x => x.RemoveUserFromRoles(id, currentRoles))
                .Returns(Task.CompletedTask);
            SetupAddToRoles(user, roles,
                IdentityResult.Failed(new IdentityError { Description = "Role error" })
            );

            //Act
            var result = await _userService.AssignRolesToUserAsync(id, roles);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.AssignRolesFailed, result.ErrorCode);
            Assert.Contains("Role error", result.ErrorMessage);
        }

        [Fact]
        public async Task AssignRolesToUserAsync_ValidRequest_ReturnSuccess()
        {
            //Arrange
            var id = Guid.NewGuid();
            var user = CreateUserForAssignRoles(id);
            var roles = new[] { "Admin", "Editor" };
            var currentRoles = new List<string> { "User" };
            SetupFindUser(id, user);
            SetupGetRoles(user, currentRoles);
            _userRepositoryMock
                .Setup(x => x.RemoveUserFromRoles(id, currentRoles))
                .Returns(Task.CompletedTask);
            SetupAddToRoles(user, roles, IdentityResult.Success);

            //Act
            var result = await _userService.AssignRolesToUserAsync(id, roles);

            //Assert
            Assert.True(result.IsSuccess);
            _userRepositoryMock.Verify(x => x.RemoveUserFromRoles(id, currentRoles), Times.Once);
            _userManagerMock.Verify(x => x.AddToRolesAsync(user, roles), Times.Once);
        }
    }
}
