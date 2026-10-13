
namespace Loan.Application.DTOs
{
    public class SanctionLetterDocumentDto
    {
        public int SanctionLetterId { get; set; }
        public int DealId { get; set; }
        public decimal LoanAmount { get; set; }
        public double InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public decimal EmiAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
