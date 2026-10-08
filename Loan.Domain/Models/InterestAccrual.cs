using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class InterestAccrual
    {
        [Key]
        public int AccrualId { get; set; }

        [ForeignKey("LoanAccount")]
        public int LoanAccountId { get; set; }

        public decimal InterestAmount { get; set; }

        public DateTime AccrualDate { get; set; }

        public int InstallmentNo { get; set; }

        public string Status { get; set; }

        public LoanAccount? LoanAccount { get; set; }
    }
}