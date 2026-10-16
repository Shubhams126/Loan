using Loan.Application.Interface;
using Loan.Application.DTOs;
using Loan.Domain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost]
        [Route("check/{customerId}/{tenureMonths}")]
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
                CustomerName = eligibility.Customer == null? string.Empty: eligibility.Customer.FirstName + " " + eligibility.Customer.LastName,
                PanNumber = eligibility.Customer?.Pan ?? string.Empty,
                MonthlyIncome = eligibility.Customer?.MonthlyIncome ?? 0,
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

        [HttpGet]
        [Route("manual-reviews")]
        public async Task<IActionResult> GetManualReviews()
        {
            var reviews = await service.GetManualReviews();
            return Ok(new ApiResponse<List<ManualReviewDto>>
            {
                Success = true,
                Message = "Manual review customers retrieved successfully",
                Data = reviews,
                Error = null
            });
        }

        [HttpPost]
        [Route("manual-review/decision")]
        public async Task<IActionResult> DecideManualReview(ManualReviewDecisionDto decision)
        {
            if (decision.Decision != "Approved" && decision.Decision != "Rejected")
            {
                return BadRequest("Decision must be Approved or Rejected");
            }

            var officerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(officerIdValue, out int officerId))
            {
                return Unauthorized("Loan officer identity was not found");
            }

            var result = await service.DecideManualReview(decision, officerId);
            if (result == null)
            {
                return NotFound("Only a customer with Manual Review status can be approved or rejected");
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Manual review decision saved successfully",
                Data = new
                {
                    result.EligibilityId,
                    result.CustomerId,
                    result.CibilScore,
                    result.IsEligible,
                    result.LoanAmount,
                    result.RejectionReason
                },
                Error = null
            });
        }
    }
}