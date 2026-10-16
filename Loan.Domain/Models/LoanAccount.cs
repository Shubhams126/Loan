using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class LoanAccount
    {
        [Key]
        public int LoanAccountId { get; set; }

        [ForeignKey("LoanDeal")]
        public int DealId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public string LoanAccountNo { get; set; }

        public decimal LoanAmount { get; set; }

        public decimal OutstandingPrincipal { get; set; }

        public string LoanStatus { get; set; }

        public decimal InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        public DateTime DisbursementDate { get; set; }

        public decimal TotalPaidAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public Customer? Customer { get; set; }

        public LoanDeal? LoanDeal { get; set; }
        public string Status { get; set; }
        // [ForeignKey("Disbursement")]
        //  public int DisbursementId { get; set; }

        // public Disbursement? Disbursement { get; set; }
    }
}