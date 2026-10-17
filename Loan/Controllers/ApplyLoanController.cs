
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
    //[Authorize]
    public class ApplyLoanController : ControllerBase
    {
        private readonly IApplyLoanService service;

        public ApplyLoanController(IApplyLoanService service)
        {
            this.service = service;
        }

        // GET: api/ApplyLoan/options
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
        //[Authorize(Roles = "User,Customer")]
        public async Task<IActionResult> ApplyLoan(
            [FromBody] LoanApplyDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    ApiResponse<LoanApplyDto>.Fail(
                        "Invalid loan application data."));
            }
        

            if (User.IsInRole("Customer") &&
                (!int.TryParse(
                    User.FindFirstValue("CustomerId"),
                    out var tokenCustomerId) ||
                 tokenCustomerId != dto.CustomerId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<LoanApplyDto>.Fail(
                        "You can only apply for a loan using your own customer account."));
            }

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
        

        // GET: api/ApplyLoan/calculate?customerId=1&loanAmount=45000&tenureYears=3
        [HttpGet]
        [Route("calculate")]
        //[Authorize(Roles = "User,Customer")]
        public async Task<IActionResult> CalculateLoan(
            [FromQuery] int customerId,
            [FromQuery] decimal loanAmount,
            [FromQuery] int tenureYears)
        {
            if (customerId <= 0)
            {
                return BadRequest(
                    ApiResponse<LoanCalculationDto>.Fail(
                        "Please enter a valid Customer ID."));
            }

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

            // If authentication is enabled, a Customer can calculate
            // eligibility only for their own CustomerId.
            if (User.IsInRole("Customer") &&
                (!int.TryParse(
                    User.FindFirstValue("CustomerId"),
                    out var tokenCustomerId) ||
                 tokenCustomerId != customerId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<LoanCalculationDto>.Fail(
                        "You can only calculate eligibility for your own customer account."));
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
