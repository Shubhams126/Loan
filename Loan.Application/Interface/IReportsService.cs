using Loan.Application.DTOs.Reports;

namespace Loan.Application.Interface
{
    public interface IReportsService
    {
        Task<ReportsDashboardDTO> GetReportsDashboardAsync(
            DateTime? fromDate,
            DateTime? toDate);
    }
}

