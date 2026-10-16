using Loan.Application.DTOs.Reports;

namespace Loan.Application.Interface
{
    public interface ILMSReportsService
    {
        Task<LMSReportsDashboardDTO> GetDashboardAsync(
        DateTime? fromDate,
        DateTime? toDate);


    Task<List<LoanPortfolioReportDTO>> GetLoanPortfolioReportAsync();

        Task<List<EMICollectionReportDTO>> GetEMICollectionReportAsync(
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<OverdueReportDTO>> GetOverdueReportAsync(
            DateTime? asOnDate);

        Task<List<DisbursementReportDTO>> GetDisbursementReportAsync(
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<PrepaymentReportDTO>> GetPrepaymentReportAsync(
            DateTime? fromDate,
            DateTime? toDate);
    }


}
