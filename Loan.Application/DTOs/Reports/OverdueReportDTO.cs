namespace Loan.Application.DTOs.Reports
{
    // Represents one overdue category in the report.
    public class OverdueReportDTO
    {
        // Overdue period, such as "1 - 30 Days" or "Above 90 Days".
        public string DaysOverdue { get; set; } = string.Empty;

        // Number of distinct loan accounts in this overdue category.
        public int Accounts { get; set; }

        // Total EMI amount overdue in this category.
        public decimal OutstandingAmount { get; set; }
    }
}

