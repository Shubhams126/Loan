using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService reportsService;

    public ReportsController(IReportsService reportsService)
        {
            this.reportsService = reportsService;
        }

        // GET: api/Reports/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetReportsDashboard(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await reportsService.GetReportsDashboardAsync(
                fromDate,
                toDate);

            return Ok(result);
        }
    }


}
