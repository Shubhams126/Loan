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
public class DisbursementController : ControllerBase
{
    private readonly IDisbursementService service;

    public DisbursementController(IDisbursementService service)
    {
        this.service = service;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<
        ApiResponse<PagedResult<DisbursementDto>>>>
        GetPendingDisbursements(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
    {
        var data = await service.GetPendingDisbursementsAsync(
            page, pageSize);

        return Ok(
            ApiResponse<PagedResult<DisbursementDto>>.Ok(
                data,
                "Pending disbursements retrieved successfully."));
    }

    [HttpPost("disburse")]
    public async Task<ActionResult<ApiResponse<DisbursementDto>>>
        DisburseLoan([FromBody] DisbursementDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse<DisbursementDto>.Fail(
                    "Invalid disbursement request."));
        }

        try
        {
            var result = await service.DisburseLoanAsync(dto);

            return Ok(
                ApiResponse<DisbursementDto>.Ok(
                    result,
                    "Loan disbursed successfully. Loan status is now Active."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                ApiResponse<DisbursementDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<DisbursementDto>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<DisbursementDto>.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<
        ApiResponse<PagedResult<DisbursementDto>>>>
        GetDisbursements(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
    {
        var data = await service.GetDisbursementsAsync(
            page, pageSize);

        return Ok(
            ApiResponse<PagedResult<DisbursementDto>>.Ok(
                data,
                "Disbursements retrieved successfully."));
    }
}
