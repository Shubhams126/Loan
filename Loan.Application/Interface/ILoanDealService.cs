
using Loan.Application.DTOs;

namespace Loan.Application.Interface
{
    public interface ILoanDealService
    {
        Task<PagedResult<LoanApplyDto>> GetPendingLoansAsync(int page, int pageSize);

        Task<LoanApplyDto?> GetLoanByIdAsync(int dealId);

        Task<bool> ApproveLoanAsync(int dealId, LoanDealReviewDto dto);

        Task<bool> RejectLoanAsync(int dealId, LoanDealReviewDto dto);

        Task<string> EvaluateCibilAsync(int dealId);
    }
}
