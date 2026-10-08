using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class EligibilityResult
    {
        [Key]
        public int EligibilityId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public int CibilScore { get; set; }

        public string? IsEligible { get; set; } = "Pending";

        public decimal LoanAmount { get; set; }
        public string? RejectionReason { get; set; }

        public Customer? Customer { get; set; }
    }
}