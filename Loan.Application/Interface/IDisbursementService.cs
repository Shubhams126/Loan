using Loan.Application.DTOs;

namespace Loan.Application.Interface
{
    public interface IDisbursementService
    {
        Task<PagedResult<DisbursementDto>>GetPendingDisbursementsAsync(int page, int pageSize);

        Task<DisbursementDto> DisburseLoanAsync(DisbursementDto dto);

        Task<PagedResult<DisbursementDto>>GetDisbursementsAsync(int page, int pageSize);
    }
}