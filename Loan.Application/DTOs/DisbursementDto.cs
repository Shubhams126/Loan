
namespace Loan.Application.DTOs
{
    public class DisbursementDto
    {
        public int DealId { get; set; }
        public decimal DisburseAmount { get; set; }
        public string BankPartner { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public string BankName { get; set; } = string.Empty;
    }
}
