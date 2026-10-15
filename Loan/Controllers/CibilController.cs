using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Loan.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CibilController : ControllerBase
    {
        ICibilService service;
        AppDbContext db;
        public CibilController(ICibilService service, AppDbContext db)
        {
            this.service = service;
            this.db = db;
        }

        [HttpPost]
        [Route("calculate/{customerId}")]
        public async Task<IActionResult> GetCibilScoreReport(int customerId)
        {
            var report = await service.GetCibilScore(customerId);

            if (report == null)
            {
                return NotFound("Customer not found");
            }

            var result = new
            {
                report.CibilReportId,
                report.CustomerId,
                CustomerName = report.Customer != null? report.Customer.FirstName + " " + report.Customer.LastName: "",
                report.PanNo,
                report.CibilScore,
                report.CheckDate
            };

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "CIBIL report generated successfully",
                Data = result,
                Error = null
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("latest/{customerId}")]
        public async Task<IActionResult> GetLatestCibil(int customerId)
        {
            var report = await service.GetLatestCibil(customerId);

            if (report == null)
            {
                return NotFound("No CIBIL report found for this customer");
            }

            var result = new
            {
                report.CibilReportId,
                report.CustomerId,
                CustomerName = report.Customer != null ? report.Customer.FirstName + " " + report.Customer.LastName : "",
                report.PanNo,
                report.CibilScore,
                report.CheckDate
            };
            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Latest CIBIL report retrieved successfully",
                Data = result,
                Error = null
            };

            return Ok(response);
        }
    }
}
