using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class ForeClosureRequest
    {
        [Key]
        public int RequestId { get; set; }

        [ForeignKey("LoanAccount")]
        public int LoanAccountId { get; set; }

        // Full / Partial

        [Required]
        public string ForeClosureType { get; set; }

        // Final Amount with charges

        public decimal ForeClosureAmount { get; set; }

        // Only for Partial Foreclosure

        public decimal PartialAmount { get; set; }

        public DateTime RequestedDate { get; set; }

        [Required]
        public DateTime ExpectedClosureDate { get; set; }

        [Required]
        public string Reason { get; set; }

        public string Status { get; set; }

        public DateTime? ApprovedDate { get; set; }

        // Payment Status

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }

        [ForeignKey("User")]
        public int? ClosedBy { get; set; }

        public LoanAccount? LoanAccount { get; set; }
    }
}