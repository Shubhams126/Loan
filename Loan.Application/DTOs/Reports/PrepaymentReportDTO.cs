namespace Loan.Application.DTOs.Reports
{
    // Represents prepayment and foreclosure payment details.
    public class PrepaymentReportDTO
    {
        // Payment type, such as "Part Payment" or "Foreclosure".
        public string Type { get; set; } = string.Empty;

        // Number of distinct loan accounts with this payment type.
        public int Accounts { get; set; }

        // Total amount paid through prepayment or foreclosure.
        public decimal PrepaymentAmount { get; set; }
    }
}

