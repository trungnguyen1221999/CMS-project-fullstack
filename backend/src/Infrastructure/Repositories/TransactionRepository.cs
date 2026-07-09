using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Royaltys.Request;
using Application.Repositories;
using Domain;
using Domain.Cores.Royalty;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<PageResult<Transaction>> GetTransactionsPagingAsync(
            TransactionHistoryRequest request
        )
        {
            var query = _context.Transactions.AsQueryable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(t =>
                    (
                        t.ToUserName.Contains(request.Keyword)
                        || t.FromUserName.Contains(request.Keyword)
                    )
                );
            }
            var fromDate = new DateTime(request.FromYear, request.FromMonth, 1);
            var toDate = new DateTime(request.ToYear, request.ToMonth, 1).AddMonths(1);
            query = query.Where(t => t.CreatedAt >= fromDate && t.CreatedAt < toDate);
            var totalTransactions = await query.CountAsync();
            query = query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize);

            return new PageResult<Transaction>
            {
                Result = await query.ToListAsync(),
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                TotalCount = totalTransactions,
            };
        }
    }
}
