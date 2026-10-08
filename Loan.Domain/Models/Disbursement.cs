using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class Disbursement
    {
        [Key]
        public int DisbursementId { get; set; }

        [ForeignKey("LoanDeal")]
        public int DealId { get; set; }

        public decimal DisburseAmount { get; set; }

        public string BankPartner { get; set; }

        public DateTime DisbursementDate { get; set; }

        public string Status { get; set; }

        public LoanDeal? LoanDeal { get; set; }
    }
}