namespace Application.Contracts.Royaltys.Request
{
    public class RoyaltyReportByUserAndMonthRequest
    {
        public Guid? UserId { get; set; }
        public int FromMonth { get; set; }
        public int FromYear { get; set; }
        public int ToMonth { get; set; }
        public int ToYear { get; set; }
    }
}
