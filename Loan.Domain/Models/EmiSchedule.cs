using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class EmiSchedule
    {
        [Key]
        public int EmiScheduleId { get; set; }

        [ForeignKey("LoanAccount")]
        public int LoanAccountId { get; set; }

        public int InstallmentNo { get; set; }

        public DateTime DueDate { get; set; }

        public decimal Emi { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal ClosingBalance { get; set; }

        public string PaymentStatus { get; set; }

        public DateTime? PaidDate { get; set; }

        public LoanAccount? LoanAccount { get; set; }
        public string Status { get; set; }
    }
}

