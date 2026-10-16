using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Loan.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/[controller]")]
//[Authorize(Roles = "User,Customer")]
public class LoanDealController : ControllerBase
{
    private readonly ILoanDealService service;

    public LoanDealController(ILoanDealService service)
    {
        this.service = service;
    }

    // GET: api/LoanDeal/pending?page=1&pageSize=10
    [HttpGet("pending")]
    public async Task<ActionResult<
        ApiResponse<PagedResult<LoanApplyDto>>>>
        GetPendingLoans(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
    {
        var data = await service.GetPendingLoansAsync(page, pageSize);

        return Ok(ApiResponse<PagedResult<LoanApplyDto>>.Ok(
            data, "Pending loan deals retrieved successfully."));
    }

    // GET: api/LoanDeal/6
    [HttpGet("{dealId:int}")]
    public async Task<ActionResult<ApiResponse<LoanApplyDto>>>
        GetLoanById(int dealId)
    {
        var loan = await service.GetLoanByIdAsync(dealId);

        if (loan == null)
        {
            return NotFound(
                ApiResponse<LoanApplyDto>.Fail(
                    "Loan deal not found."));
        }

        return Ok(ApiResponse<LoanApplyDto>.Ok(
            loan, "Loan deal retrieved successfully."));
    }

    [HttpPost("{dealId:int}/evaluate-cibil")]
    public async Task<ActionResult<ApiResponse<object>>> EvaluateCibil(int dealId)
    {
        try
        {
            var result = await service.EvaluateCibilAsync(dealId);

            if (result == "Loan deal not found.")
            {
                return NotFound(
                    ApiResponse<object>.Fail(result));
            }

            if (result.StartsWith("Cannot evaluate"))
            {
                return BadRequest(
                    ApiResponse<object>.Fail(result));
            }

            string status;

            if (result.StartsWith("Loan automatically approved"))
            {
                status = "Approved";
            }
            else if (result.StartsWith("Loan sent for manual review") ||
                     result.StartsWith("No CIBIL report found"))
            {
                status = "Manual Review";
            }
            else if (result.StartsWith("Loan automatically rejected"))
            {
                status = "Rejected";
            }
            else
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Unable to determine the CIBIL decision."));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        DealId = dealId,
                        Status = status,
                        Decision = result
                    },
                    "CIBIL evaluation completed successfully."));
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(
                    "An error occurred while evaluating the CIBIL score."));
        }
    }

    // PUT: api/LoanDeal/6/approve
    [HttpPut("{dealId:int}/approve")]
    public async Task<ActionResult<ApiResponse<object>>>
        ApproveLoan(
            int dealId,
            [FromBody] LoanDealReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Invalid loan approval request."));
        }

        // Your current service requires officerId.
        // This value is for local testing only.
        const int officerId = 1;

        if (!await service.ApproveLoanAsync(
                dealId, dto, officerId))
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Loan not found, loan is not awaiting review, or approved amount is invalid."));
        }

        return Ok(ApiResponse<object>.Ok(
            new
            {
                DealId = dealId,
                Status = "Approved"
            },
            "Loan approved successfully."));
    }

    // PUT: api/LoanDeal/6/reject
    [HttpPut("{dealId:int}/reject")]
    public async Task<ActionResult<ApiResponse<object>>>
        RejectLoan(
            int dealId,
            [FromBody] LoanDealReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Invalid loan rejection request."));
        }

        // Your current service requires officerId.
        // This value is for local testing only.
        const int officerId = 1;

        if (!await service.RejectLoanAsync(
                dealId, dto, officerId))
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Loan not found, loan is not awaiting review, or rejection reason is missing."));
        }

        return Ok(ApiResponse<object>.Ok(
            new
            {
                DealId = dealId,
                Status = "Rejected"
            },
            "Loan rejected successfully."));
    }
}
