namespace Loan.Application.DTOs.Reports
{
    // Represents the loan portfolio report for one loan product.
    public class LoanPortfolioReportDTO
    {
        // Name or type of the loan product.
        public string Product { get; set; } = string.Empty;

        // Number of active loans under this product.
        public int ActiveLoans { get; set; }

        // Total outstanding principal for this product.
        public decimal OutstandingAmount { get; set; }

        // Percentage of the total portfolio outstanding amount.
        public decimal Percentage { get; set; }
    }
}
