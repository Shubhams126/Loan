
namespace Loan.Domain.Models
{
    public class DealReview
    {
        public int DealReviewId { get; set; }
        public int DealId { get; set; }
        public int OfficerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime ReviewedAt { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public LoanDeal? LoanDeal { get; set; }
    }
}

