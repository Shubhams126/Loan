
namespace Loan.Application.DTOs
{
    public class LoanCalculationDto
    {
        public double InterestRate { get; set; }
        public decimal EmiAmount { get; set; }
        public string? Message { get; set; }
    }
}
