using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class DealReview
    {
        [Key]
        public int ReviewId { get; set; }

        [ForeignKey("LoanDeal")]
        public int DealId { get; set; }

        [ForeignKey("User")]
        public int OfficerId { get; set; }

        public string Status { get; set; }

        public LoanDeal? LoanDeal { get; set; }

        public User? User { get; set; }
    }
}