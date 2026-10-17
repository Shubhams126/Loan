namespace Loan.Application.DTOs.Reports
{
    // Represents loan disbursement details for a particular month.
    public class DisbursementReportDTO
    {
        // Month of disbursement in YYYY-MM format.
        public string Month { get; set; } = string.Empty;

        // Number of loans disbursed during that month.
        public int LoansDisbursed { get; set; }

        // Total amount disbursed during that month.
        public decimal DisbursementAmount { get; set; }
    }
}

