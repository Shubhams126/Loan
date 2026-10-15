
using System.Security.Claims;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplyLoanController : ControllerBase
    {
        private readonly IApplyLoanService service;

        public ApplyLoanController(IApplyLoanService service)
        {
            this.service = service;
        }

        [HttpGet("options")]
        public IActionResult GetLoanOptions()
        {
            return Ok(new
            {
                Success = true,
                Data = service.GetLoanOptions()
            });
        }


        [HttpPost("apply")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ApplyLoan(
            [FromBody] LoanApplyDto dto)
        {
            var result = await service.ApplyLoanAsync(dto);

            if (result.DealId == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new
            {
                Success = true,
                Message = result.Message,
                Data = result
            });
        }

        [HttpGet("calculate")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CalculateLoan(
            [FromQuery] int customerId,
            [FromQuery] decimal loanAmount,
            [FromQuery] int tenureYears)
        {
            var result = await service.CalculateLoanAsync(
                customerId,
                loanAmount,
                tenureYears);

            if (result.InterestRate == 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new
            {
                Success = true,
                Data = result
            });
        }

        [HttpGet("pending")]

        public async Task<IActionResult> GetPendingLoans()
        {
            var loans = await service.GetPendingLoansAsync();

            return Ok(new
            {
                Success = true,
                Data = loans
            });
        }

        [HttpGet("{dealId:int}")]
   
        public async Task<IActionResult> GetLoanById(int dealId)
        {
            var loan = await service.GetLoanByIdAsync(dealId);

            if (loan == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Loan application not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Data = loan
            });
        }

        [HttpPut("{dealId:int}/approve")]
    
        public async Task<IActionResult> ApproveLoan(
            int dealId,
            [FromBody] LoanDealReviewDto dto)
        {
            int? officerId = GetOfficerId();

            if (officerId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Officer ID was not found in the login claims."
                });
            }

            bool approved = await service.ApproveLoanAsync(
                dealId,
                dto,
                officerId.Value);

            if (!approved)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message =
                        "Loan not found, not pending, or approved amount is invalid."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Loan approved successfully."
            });
        }

        [HttpPut("{dealId:int}/reject")]

        public async Task<IActionResult> RejectLoan(
            int dealId,
            [FromBody] LoanDealReviewDto dto)
        {
            int? officerId = GetOfficerId();

            if (officerId == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Officer ID was not found in the login claims."
                });
            }

            bool rejected = await service.RejectLoanAsync(
                dealId,
                dto,
                officerId.Value);

            if (!rejected)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message =
                        "Loan not found, not pending, or rejection reason is missing."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Loan rejected successfully."
            });
        }

        private int? GetOfficerId()
        {
            string? id = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(id, out int officerId))
            {
                return officerId;
            }

            return null;
        }
    }
}
