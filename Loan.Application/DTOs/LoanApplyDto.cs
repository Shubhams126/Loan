namespace Loan.Application.DTOs
{
    public class LoanApplyDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }

        public int DealId { get; set; }

        public string LoanType { get; set; }

        public decimal LoanAmount { get; set; }

        public int TenureMonths { get; set; }

        public int EmiDay { get; set; }

        public string BankName { get; set; }

        public string BankAccountNumber { get; set; }

        public string IFSCCode { get; set; }

        public double InterestRate { get; set; }

        public decimal EmiAmount { get; set; }

        public string? CurrentStatus { get; set; }

        public string? RejectionReason { get; set; }
        public DateTime AppliedDate { get; set; }
        public string? Message { get; set; }

    }
}
