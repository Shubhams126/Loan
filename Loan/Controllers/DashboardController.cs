using Loan.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  //  [Authorize(Roles = "LoanOfficer")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            var dashboard = dashboardService.GetDashboardData();

            return Ok(dashboard);
        }
    }
}
