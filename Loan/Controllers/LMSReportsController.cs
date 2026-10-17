using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LMSReportsController : ControllerBase
    {
        private readonly ILMSReportsService reportsService;


    public LMSReportsController(ILMSReportsService reportsService)
        {
            this.reportsService = reportsService;
        }

        // GET: api/LMSReports/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await reportsService.GetDashboardAsync(
                fromDate,
                toDate);

            return Ok(result);
        }

        // GET: api/LMSReports/loan-portfolio
        [HttpGet("loan-portfolio")]
        public async Task<IActionResult> GetLoanPortfolioReport()
        {
            var result = await reportsService.GetLoanPortfolioReportAsync();

            return Ok(result);
        }

        // GET: api/LMSReports/emi-collection
        [HttpGet("emi-collection")]
        public async Task<IActionResult> GetEMICollectionReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await reportsService.GetEMICollectionReportAsync(
                fromDate,
                toDate);

            return Ok(result);
        }

        // GET: api/LMSReports/overdue
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueReport(
            [FromQuery] DateTime? asOnDate)
        {
            var result = await reportsService.GetOverdueReportAsync(asOnDate);

            return Ok(result);
        }

        // GET: api/LMSReports/disbursement
        [HttpGet("disbursement")]
        public async Task<IActionResult> GetDisbursementReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await reportsService.GetDisbursementReportAsync(
                fromDate,
                toDate);

            return Ok(result);
        }

        // GET: api/LMSReports/prepayment
        [HttpGet("prepayment")]
        public async Task<IActionResult> GetPrepaymentReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await reportsService.GetPrepaymentReportAsync(
                fromDate,
                toDate);

            return Ok(result);
        }
    }


}
