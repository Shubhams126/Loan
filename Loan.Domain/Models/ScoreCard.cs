using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class ScoreCard
    {
        [Key]
        public int ScoreCardId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public string RiskCategory { get; set; }

        public decimal EligibleLoanAmount { get; set; }

        public Customer? Customer { get; set; }
    }
}