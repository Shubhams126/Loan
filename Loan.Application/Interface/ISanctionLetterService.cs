
using Loan.Application.DTOs;

namespace Loan.Application.Interface
{
    public interface ISanctionLetterService
    {
        Task<List<LoanApplyDto>> GetApprovedLoansAsync();

        Task<PagedResult<ApprovedLoanDto>> GetApprovedLoansAsync(int page, int pageSize);

        Task<SanctionLetterDocumentDto> GenerateSanctionLetterAsync(int dealId);

        Task<SanctionLetterDocumentDto?> GetSanctionLetterAsync(int dealId);
    }
}

