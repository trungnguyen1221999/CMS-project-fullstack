using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Common;

namespace Application.Contracts.Royaltys.Request
{
    public class TransactionHistoryRequest : PagingRequest
    {
        public int FromMonth { get; set; }
        public int FromYear { get; set; }
        public int ToMonth { get; set; }
        public int ToYear { get; set; }
    }
}
