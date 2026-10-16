using Loan.Application.DTOs;

namespace Loan.Application.Interface;

public interface IApplyLoanService
{
    object GetLoanOptions();
    Task<LoanCalculationDto> CalculateLoanAsync(int customerId, decimal loanAmount, int tenureYears);
    Task<LoanApplyDto> ApplyLoanAsync(LoanApplyDto dto);
}
