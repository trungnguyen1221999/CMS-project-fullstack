using Application.Contracts.Royaltys.Request;
using Domain;
using Domain.Cores.Royalty;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Test.Royalty.Tests
{
    public class TransactionRepository_GetTransactionsPagingTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TransactionRepository _repository;

        private readonly Guid _adminId = Guid.NewGuid();
        private readonly Guid _authorA = Guid.NewGuid();
        private readonly Guid _authorB = Guid.NewGuid();

        public TransactionRepository_GetTransactionsPagingTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TransactionRepository(_context);

            SeedData();
        }

        private void SeedData()
        {
            var transactions = new List<Transaction>
            {
                CreateTransaction(_adminId, "admin", _authorA, "authorA", 100m, new DateTime(2025, 1, 15)),
                CreateTransaction(_adminId, "admin", _authorA, "authorA", 200m, new DateTime(2025, 2, 10)),
                CreateTransaction(_adminId, "admin", _authorB, "authorB", 150m, new DateTime(2025, 2, 20)),
                CreateTransaction(_adminId, "admin", _authorA, "authorA", 300m, new DateTime(2025, 3, 5)),
                CreateTransaction(_adminId, "admin", _authorB, "authorB", 250m, new DateTime(2025, 4, 1)),
            };

            _context.Transactions.AddRange(transactions);
            _context.SaveChanges();
        }

        private static Transaction CreateTransaction(
            Guid fromUserId,
            string fromUserName,
            Guid toUserId,
            string toUserName,
            decimal amount,
            DateTime createdAt
        ) =>
            new()
            {
                Id = Guid.NewGuid(),
                FromUserId = fromUserId,
                FromUserName = fromUserName,
                ToUserId = toUserId,
                ToUserName = toUserName,
                Amount = amount,
                TransactionType = TransactionType.RoyaltyPay,
                CreatedAt = createdAt,
            };

        [Fact]
        public async Task ReturnsTransactionsInDateRange()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 2,
                ToYear = 2025,
                CurrentPage = 1,
                PageSize = 10,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Result.Count);
        }

        [Fact]
        public async Task FiltersByKeyword_MatchesUserName()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 4,
                ToYear = 2025,
                Keyword = "authorB",
                CurrentPage = 1,
                PageSize = 10,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Result, t =>
                Assert.True(
                    t.ToUserName.Contains("authorB") || t.FromUserName.Contains("authorB")
                )
            );
        }

        [Fact]
        public async Task PagingWorks_ReturnsCorrectPage()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
                CurrentPage = 1,
                PageSize = 2,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            Assert.Equal(4, result.TotalCount);
            Assert.Equal(2, result.Result.Count);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task PagingWorks_SecondPage()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
                CurrentPage = 2,
                PageSize = 2,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            Assert.Equal(4, result.TotalCount);
            Assert.Equal(2, result.Result.Count);
            Assert.Equal(2, result.CurrentPage);
        }

        [Fact]
        public async Task OrderedByCreatedAtDescending()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 4,
                ToYear = 2025,
                CurrentPage = 1,
                PageSize = 10,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            for (int i = 1; i < result.Result.Count; i++)
            {
                Assert.True(result.Result[i - 1].CreatedAt >= result.Result[i].CreatedAt);
            }
        }

        [Fact]
        public async Task NoTransactionsInRange_ReturnsEmpty()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 6,
                FromYear = 2025,
                ToMonth = 8,
                ToYear = 2025,
                CurrentPage = 1,
                PageSize = 10,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Result);
        }

        [Fact]
        public async Task NoKeywordMatch_ReturnsEmpty()
        {
            var request = new TransactionHistoryRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 4,
                ToYear = 2025,
                Keyword = "nonexistent",
                CurrentPage = 1,
                PageSize = 10,
            };

            var result = await _repository.GetTransactionsPagingAsync(request);

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
