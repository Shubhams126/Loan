
using Loan.Application.DTOs;

namespace Loan.Application.Interface
{
    public interface IApplyLoanService
    {
        object GetLoanOptions();

        Task<LoanCalculationDto> CalculateLoanAsync(
            int customerId,
            decimal loanAmount,
            int tenureYears);

        Task<LoanApplyDto> ApplyLoanAsync(LoanApplyDto dto);

        Task<List<LoanApplyDto>> GetPendingLoansAsync();

        Task<LoanApplyDto?> GetLoanByIdAsync(int dealId);

        Task<bool> ApproveLoanAsync(
            int dealId,
            LoanDealReviewDto dto,
            int officerId);

        Task<bool> RejectLoanAsync(
            int dealId,
            LoanDealReviewDto dto,
            int officerId);
    }
}
