
using System.Security.Claims;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Loan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("api")]
    [Authorize]
    public class ApplyLoanController : ControllerBase
    {
        private readonly IApplyLoanService service;

        public ApplyLoanController(IApplyLoanService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("options")]
        [AllowAnonymous]
        public IActionResult GetLoanOptions()
        {
            var result = service.GetLoanOptions();

            var response = ApiResponse<object>.Ok(
                result,
                "Loan options loaded successfully.");

            return Ok(response);
        }

        // POST: api/ApplyLoan/apply
        [HttpPost]
        [Route("apply")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ApplyLoan(
            [FromBody] LoanApplyDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    ApiResponse<LoanApplyDto>.Fail(
                        "Invalid loan application data."));
            }

            // Get CustomerId from the authenticated JWT.
            if (!int.TryParse(
                User.FindFirstValue("CustomerId"),
                out var customerId) ||
                customerId <= 0)
            {
                return Unauthorized(
                    ApiResponse<LoanApplyDto>.Fail(
                        "Customer ID not found in authentication token."));
            }

            // Always use the authenticated customer's ID.
            dto.CustomerId = customerId;

            var result = await service.ApplyLoanAsync(dto);

            if (result.DealId == 0)
            {
                return BadRequest(
                    ApiResponse<LoanApplyDto>.Fail(
                        result.Message ??
                        "Loan application could not be submitted."));
            }

            return Ok(
                ApiResponse<LoanApplyDto>.Ok(
                    result,
                    result.Message ??
                    "Loan application submitted successfully."));
        }

        // GET: api/ApplyLoan/calculate
        [HttpGet]
        [Route("calculate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CalculateLoan(
            [FromQuery] decimal loanAmount,
            [FromQuery] int tenureYears)
        {
            if (loanAmount <= 0)
            {
                return BadRequest(
                    ApiResponse<LoanCalculationDto>.Fail(
                        "Loan amount must be greater than zero."));
            }

            if (tenureYears <= 0)
            {
                return BadRequest(
                    ApiResponse<LoanCalculationDto>.Fail(
                        "Please select a valid loan tenure."));
            }

            // Get CustomerId from the authenticated JWT.
            if (!int.TryParse(
                User.FindFirstValue("CustomerId"),
                out var customerId) ||
                customerId <= 0)
            {
                return Unauthorized(
                    ApiResponse<LoanCalculationDto>.Fail(
                        "Customer ID not found in authentication token."));
            }

            var result = await service.CalculateLoanAsync(
                customerId,
                loanAmount,
                tenureYears);

            if (result.InterestRate == 0)
            {
                return BadRequest(
                    ApiResponse<LoanCalculationDto>.Fail(
                        result.Message ??
                        "Loan calculation failed."));
            }

            return Ok(
                ApiResponse<LoanCalculationDto>.Ok(
                    result,
                    result.Message ??
                    "Loan calculation completed successfully."));
        }
    }
}
