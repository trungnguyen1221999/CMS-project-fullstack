using Application.Constants;
using Application.Contracts.Royaltys.Request;
using Application.Repositories;
using Domain;
using Domain.Cores.Royalty;
using Moq;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Royalty.Tests
{
    public partial class RoyaltyServiceTest
    {
        private static TransactionHistoryRequest CreateValidTransactionRequest() =>
            new()
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
                CurrentPage = 1,
                PageSize = 10,
            };

        [Fact]
        public async Task GetTransactionHistoryAsync_NoPermission_ThrowsForbidden()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidTransactionRequest();

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _royaltyService.GetTransactionHistoryAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Royalty.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task GetTransactionHistoryAsync_InvalidDateRange_FromYearGreater_ThrowsBadRequest()
        {
            var userId = Guid.NewGuid();
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2026,
                ToMonth = 3,
                ToYear = 2025,
            };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _royaltyService.GetTransactionHistoryAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Royalty.InvalidDateRange, ex.ErrorCode);
        }

        [Fact]
        public async Task GetTransactionHistoryAsync_InvalidDateRange_SameYearFromMonthGreater_ThrowsBadRequest()
        {
            var userId = Guid.NewGuid();
            var request = new TransactionHistoryRequest
            {
                FromMonth = 6,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
            };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _royaltyService.GetTransactionHistoryAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Royalty.InvalidDateRange, ex.ErrorCode);
        }

        [Fact]
        public async Task GetTransactionHistoryAsync_Success_ReturnsPageResult()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidTransactionRequest();
            var mockTransactionRepo = new Mock<ITransactionRepository>();

            var expectedResult = new PageResult<Transaction>
            {
                Result =
                [
                    new Transaction
                    {
                        Id = Guid.NewGuid(),
                        FromUserId = userId,
                        FromUserName = "admin",
                        ToUserId = Guid.NewGuid(),
                        ToUserName = "author",
                        Amount = 100,
                        TransactionType = TransactionType.RoyaltyPay,
                        CreatedAt = new DateTime(2025, 2, 15),
                    },
                ],
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 1,
            };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            mockTransactionRepo
                .Setup(x => x.GetTransactionsPagingAsync(request))
                .ReturnsAsync(expectedResult);

            _mockUnitOfWork.Setup(u => u.Transactions).Returns(mockTransactionRepo.Object);

            var result = await _royaltyService.GetTransactionHistoryAsync(request, userId);

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Result);
            Assert.Equal("admin", result.Result[0].FromUserName);
        }

        [Fact]
        public async Task GetTransactionHistoryAsync_EmptyResult_ReturnsEmptyPageResult()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidTransactionRequest();
            var mockTransactionRepo = new Mock<ITransactionRepository>();

            var expectedResult = new PageResult<Transaction>
            {
                Result = [],
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 0,
            };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            mockTransactionRepo
                .Setup(x => x.GetTransactionsPagingAsync(request))
                .ReturnsAsync(expectedResult);

            _mockUnitOfWork.Setup(u => u.Transactions).Returns(mockTransactionRepo.Object);

            var result = await _royaltyService.GetTransactionHistoryAsync(request, userId);

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Result);
        }
    }
}
