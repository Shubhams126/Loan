using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class LoanDeal
    {
        [Key]
        public int DealId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public string LoanType { get; set; }

        public decimal LoanAmount { get; set; }

        public double InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        [Required]
        public string BankName { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{9,18}$",
            ErrorMessage = "Bank Account Number must be between 9 to 18 digits")]
        public string BankAccountNumber { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$",
            ErrorMessage = "Invalid IFSC Code")]

        public string IFSCCode { get; set; }

        [Required]
        public int EmiDay { get; set; }

      
        public decimal ApprovedAmount { get; set; }

        public string CurrentStatus { get; set; } = "Pending";

        public string? RejectionReason { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public Customer? Customer { get; set; }

        public ICollection<DealReview>? DealReviews { get; set; }

        public ICollection<SanctionLetter>? SanctionLetters { get; set; }

        public ICollection<Disbursement>? Disbursements { get; set; }


    }
}