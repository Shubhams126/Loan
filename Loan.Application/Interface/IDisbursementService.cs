using Loan.Application.DTOs;

namespace Loan.Application.Interface
{
    public interface IDisbursementService
    {
        Task<List<DisbursementDto>> GetPendingDisbursementsAsync();

        Task<DisbursementDto> DisburseLoanAsync(DisbursementDto dto);

        Task<List<DisbursementDto>> GetDisbursementsAsync();
    }
}
