namespace Loan.Application.DTOs.Reports
{
    // Represents one row in the EMI Collection Report.
    public class EMICollectionReportDTO
    {
        // Description of the report row, such as EMI Billed,
        // EMI Collected, or EMI Pending.
        public string Particulars { get; set; } = string.Empty;

        // Number of EMI records included in this row.
        public int Count { get; set; }

        // Total EMI amount for this row.
        public decimal Amount { get; set; }

        // Percentage of the total billed EMI amount.
        public decimal Percentage { get; set; }
    }
}

