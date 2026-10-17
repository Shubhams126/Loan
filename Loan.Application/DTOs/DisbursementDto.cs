namespace Loan.Application.DTOs
{
    public class DisbursementDto
    {
        public int DealId { get; set; }
        public decimal DisburseAmount { get; set; }
        public string BankPartner { get; set; } = string.Empty;
    }
}