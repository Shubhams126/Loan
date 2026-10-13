using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EligibilityController : ControllerBase
    {
        IEligibilityService service;
        public EligibilityController(IEligibilityService service)
        {
            this.service = service;
        }

        [HttpPost("check/{customerId}/{tenureMonths}")]
        public async Task<IActionResult> CheckEligibility(int customerId, int tenureMonths)
        {
            var eligibility = await service.CheckEligibility(customerId, tenureMonths);

            if (eligibility == null)
            {
                return NotFound("Some error occurred while checking eligibility");
            }

            var result = new
            {
                eligibility.EligibilityId,
                eligibility.CustomerId,
                eligibility.CibilScore,
                eligibility.IsEligible,
                eligibility.LoanAmount,
                eligibility.RejectionReason
            };

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Eligibility checked successfully",
                Data = result,
                Error = null
            };

            return Ok(response);
        }
    }
}