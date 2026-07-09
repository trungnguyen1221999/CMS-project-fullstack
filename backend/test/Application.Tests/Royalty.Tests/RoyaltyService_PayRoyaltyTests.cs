using Application.Constants;
using Application.Repositories;
using Domain.Cores.Content;
using Domain.Cores.Royalty;
using Microsoft.AspNetCore.Identity;
using Moq;
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Royalty.Tests
{
    public partial class RoyaltyServiceTest
    {
        private static AppUser CreateUser(Guid id, string userName, decimal royaltyAmount = 50m) =>
            new()
            {
                Id = id,
                UserName = userName,
                FirstName = userName,
                LastName = "Test",
                RoyaltyAmountPerPost = royaltyAmount,
                Balance = 0,
            };

        [Fact]
        public async Task PayRoyaltyForUserAsync_FromUserNotFound_ThrowsNotFound()
        {
            var fromUserId = Guid.NewGuid();
            var toUserId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(fromUserId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId)
            );
            Assert.Contains(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task PayRoyaltyForUserAsync_ToUserNotFound_ThrowsNotFound()
        {
            var fromUserId = Guid.NewGuid();
            var toUserId = Guid.NewGuid();
            var fromUser = CreateUser(fromUserId, "admin");

            _mockUserManager
                .Setup(x => x.FindByIdAsync(fromUserId.ToString()))
                .ReturnsAsync(fromUser);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(toUserId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId)
            );
            Assert.Contains(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task PayRoyaltyForUserAsync_NoPermission_ThrowsForbidden()
        {
            var fromUserId = Guid.NewGuid();
            var toUserId = Guid.NewGuid();
            var fromUser = CreateUser(fromUserId, "admin");
            var toUser = CreateUser(toUserId, "author");

            _mockUserManager
                .Setup(x => x.FindByIdAsync(fromUserId.ToString()))
                .ReturnsAsync(fromUser);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(toUserId.ToString()))
                .ReturnsAsync(toUser);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyPayPermission(fromUserId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId)
            );
            Assert.Equal(ErrorMessages.Royalty.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task PayRoyaltyForUserAsync_NoUnpaidPosts_ThrowsBadRequest()
        {
            var fromUserId = Guid.NewGuid();
            var toUserId = Guid.NewGuid();
            var fromUser = CreateUser(fromUserId, "admin");
            var toUser = CreateUser(toUserId, "author");

            _mockUserManager
                .Setup(x => x.FindByIdAsync(fromUserId.ToString()))
                .ReturnsAsync(fromUser);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(toUserId.ToString()))
                .ReturnsAsync(toUser);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyPayPermission(fromUserId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.GetListUnpaidPublishPosts(toUserId))
                .ReturnsAsync([]);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId)
            );
            Assert.Equal(ErrorMessages.Royalty.NoUnpaidPosts, ex.ErrorCode);
        }

        [Fact]
        public async Task PayRoyaltyForUserAsync_Success_UpdatesPostsAndCreatesTransaction()
        {
            var fromUserId = Guid.NewGuid();
            var toUserId = Guid.NewGuid();
            var fromUser = CreateUser(fromUserId, "admin");
            var toUser = CreateUser(toUserId, "author", royaltyAmount: 100m);
            var mockTransactionRepo = new Mock<ITransactionRepository>();

            var unpaidPosts = new List<AppPost>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = toUserId,
                    Status = PostStatus.Published,
                    IsPaid = false,
                    Name = "Post 1",
                    Slug = "post-1",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = toUserId,
                    Status = PostStatus.Published,
                    IsPaid = false,
                    Name = "Post 2",
                    Slug = "post-2",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                },
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(fromUserId.ToString()))
                .ReturnsAsync(fromUser);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(toUserId.ToString()))
                .ReturnsAsync(toUser);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyPayPermission(fromUserId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.GetListUnpaidPublishPosts(toUserId))
                .ReturnsAsync(unpaidPosts);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUnitOfWork.Setup(u => u.Transactions).Returns(mockTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            var result = await _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId);

            Assert.True(result);

            // Verify posts are marked as paid
            Assert.All(unpaidPosts, p =>
            {
                Assert.True(p.IsPaid);
                Assert.NotNull(p.PaidDate);
                Assert.Equal(100m, p.RoyaltyAmount);
            });

            // Verify user balance updated: 2 posts * 100 = 200
            Assert.Equal(200m, toUser.Balance);

            // Verify transaction was created
            mockTransactionRepo.Verify(
                x =>
                    x.Add(
                        It.Is<Transaction>(t =>
                            t.FromUserId == fromUserId
                            && t.ToUserId == toUserId
                            && t.Amount == 200m
                            && t.TransactionType == TransactionType.RoyaltyPay
                        )
                    ),
                Times.Once
            );

            // Verify CompleteAsync was called
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task PayRoyaltyForUserAsync_SaveFails_ReturnsFalse()
        {
            var fromUserId = Guid.NewGuid();
            var toUserId = Guid.NewGuid();
            var fromUser = CreateUser(fromUserId, "admin");
            var toUser = CreateUser(toUserId, "author", royaltyAmount: 50m);
            var mockTransactionRepo = new Mock<ITransactionRepository>();

            var unpaidPosts = new List<AppPost>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = toUserId,
                    Status = PostStatus.Published,
                    IsPaid = false,
                    Name = "Post 1",
                    Slug = "post-1",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                },
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(fromUserId.ToString()))
                .ReturnsAsync(fromUser);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(toUserId.ToString()))
                .ReturnsAsync(toUser);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyPayPermission(fromUserId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.GetListUnpaidPublishPosts(toUserId))
                .ReturnsAsync(unpaidPosts);

            _mockUserManager
                .Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUnitOfWork.Setup(u => u.Transactions).Returns(mockTransactionRepo.Object);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

            var result = await _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId);

            Assert.False(result);
        }
    }
}
