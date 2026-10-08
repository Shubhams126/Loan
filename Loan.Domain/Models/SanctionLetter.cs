using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class SanctionLetter
    {
        [Key]
        public int SanctionId { get; set; }

        [ForeignKey("LoanDeal")]
        public int DealId { get; set; }

        public decimal LoanAmount { get; set; }

        public double InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        public DateTime CreatedAt { get; set; }
           = DateTime.Now;


        public LoanDeal? LoanDeal { get; set; }
    }
}