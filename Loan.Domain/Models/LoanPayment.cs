using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class LoanPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [ForeignKey("LoanAccount")]
        public int LoanAccountId { get; set; }

        public string PaymentName { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal PaidAmount { get; set; }

        public string PaymentMode { get; set; }

        public string TransactionReference { get; set; }

        public decimal PrincipalPaid { get; set; }

        public decimal InterestPaid { get; set; }

        public decimal PenaltyPaid { get; set; }

        public string PaymentStatus { get; set; }

        public LoanAccount? LoanAccount { get; set; }
        public decimal? Amount { get; set; }
    }
}