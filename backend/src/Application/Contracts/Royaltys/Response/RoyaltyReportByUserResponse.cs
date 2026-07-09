namespace Application.Contracts.Royaltys.Response
{
    public class RoyaltyReportByUserResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int NumberOfDraftPosts { get; set; }
        public int NumberOfWaitingApprovalPosts { get; set; }
        public int NumberOfRejectedPosts { get; set; }
        public int NumberOfUnpaidPublishPosts { get; set; }
        public int NumberOfPaidPublishPosts { get; set; }
        public int NumberOfPublishPosts { get; set; }
    }
}
