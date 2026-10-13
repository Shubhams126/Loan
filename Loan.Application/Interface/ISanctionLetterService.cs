
using Loan.Application.DTOs;

namespace Loan.Application.Interface
{
    public interface ISanctionLetterService
    {
        Task<List<ApprovedLoanDto>> GetApprovedLoansAsync();

        Task<SanctionLetterDocumentDto> GenerateSanctionLetterAsync(
            int dealId);

        Task<SanctionLetterDocumentDto?> GetSanctionLetterAsync(
            int dealId);
    }
}
