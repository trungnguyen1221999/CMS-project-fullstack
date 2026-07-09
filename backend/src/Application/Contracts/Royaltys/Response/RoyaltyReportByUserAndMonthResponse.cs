namespace Application.Contracts.Royaltys.Response
{
    public class RoyaltyReportByUserAndMonthResponse : RoyaltyReportByUserResponse
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
