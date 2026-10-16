
namespace Loan.Application.DTOs
{
    public class ApprovedLoanDto
    {
        public int DealId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public double InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public decimal EmiAmount { get; set; }
        public DateTime AppliedDate { get; set; }
    }
}

