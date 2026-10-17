using System.ComponentModel.DataAnnotations;

namespace Loan.Domain.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "First Name is required")]

        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        // AGE VALIDATION
        [Required(ErrorMessage = "Age is required")]
        [Range(18, 60, ErrorMessage = "Age must be between 18 and 60")]

        public int Age { get; set; }

        // EMAIL VALIDATION
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        // MOBILE NUMBER VALIDATION
        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string MobileNo { get; set; }

        // PAN CARD VALIDATION
        [Required(ErrorMessage = "PAN Number is required")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN Number")]
        public string Pan { get; set; }

        // AADHAAR VALIDATION
        [Required(ErrorMessage = "Aadhaar Number is required")]
        [RegularExpression(@"^[0-9]{12}$",
            ErrorMessage = "Aadhaar must be 12 digits")]
        public string AadhaarNo { get; set; }

        [Required(ErrorMessage = "Employment Type is required")]
        public string EmploymentType { get; set; }

        [Required(ErrorMessage = "Monthly Income is required")]
        [Range(1000, 10000000, ErrorMessage = "Enter valid income")]
        public decimal MonthlyIncome { get; set; }

        // TOTAL MONTHLY DEBT PAYMENT VALIDATION
        [Required(ErrorMessage = "Total Monthly Debt Payment is required")]
        [Range(0, 10000000, ErrorMessage = "Enter valid debt amount")]
        public decimal TotalMonthlyDebt { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        // Navigation Properties
        public ICollection<KycDocument>? KycDocuments { get; set; }

        public ICollection<CibilReport>? CibilReports { get; set; }

        public ICollection<EligibilityResult>? EligibilityResults { get; set; }

        public ICollection<ScoreCard>? ScoreCards { get; set; }

        public ICollection<LoanDeal>? LoanDeals { get; set; }
    }
}