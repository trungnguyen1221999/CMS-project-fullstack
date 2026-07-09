using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Royaltys.Request
{
    public class RoyaltyReportByUserRequest
    {
        public Guid? UserId { get; set; }
        public int FromMonth { get; set; }
        public int FromYear { get; set; }
        public int ToMonth { get; set; }
        public int ToYear { get; set; }
    }
}
