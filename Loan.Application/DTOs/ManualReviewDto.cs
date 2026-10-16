namespace Loan.Application.DTOs
{
    public class ManualReviewDto
    {
        public int EligibilityId { get; set; }
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string Pan { get; set; } = string.Empty;
        public string AadhaarNo { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public decimal TotalMonthlyDebt { get; set; }
        public int CibilReportId { get; set; }
        public int CibilScore { get; set; }
        public DateTime CibilCheckDate { get; set; }
        public string CibilStatus { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ManualReviewDecisionDto
    {
        public int CustomerId { get; set; }
        public string Decision { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}
