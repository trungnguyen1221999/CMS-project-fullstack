using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Royaltys.Request;
using Domain;
using Domain.Cores.Royalty;

namespace Application.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction, Guid>
    {
        Task<PageResult<Transaction>> GetTransactionsPagingAsync(TransactionHistoryRequest request);
    }
}
