using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class LoanClosure
    {
        [Key]
        public int ClosureId { get; set; }

        [ForeignKey("LoanAccount")]
        public int LoanAccountId { get; set; }

        public string ClosureType { get; set; }

        public decimal FinalSettlementAmount { get; set; }

        public DateTime ClosureDate { get; set; }

        [ForeignKey("User")]
        public int ClosedBy { get; set; }

        public string Remarks { get; set; }

        public string ClosureStatus { get; set; }

        public LoanAccount? LoanAccount { get; set; }

        public User? User { get; set; }
    }
}