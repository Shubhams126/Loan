
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Loan.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/[controller]")]
public class SanctionLetterController : ControllerBase
{
    private readonly ISanctionLetterService service;
    private readonly IWebHostEnvironment env;

    public SanctionLetterController(
        ISanctionLetterService service,
        IWebHostEnvironment env)
    {
        this.service = service;
        this.env = env;
    }

    [HttpGet("approved-loans")]
    public async Task<ActionResult<ApiResponse<PagedResult<ApprovedLoanDto>>>>
        GetApprovedLoans(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
    {
        var data = await service.GetApprovedLoansAsync(page, pageSize);

        return Ok(ApiResponse<PagedResult<ApprovedLoanDto>>.Ok(
            data, "Approved loans retrieved."));
    }

    [HttpPost("{dealId:int}/generate")]
    public async Task<ActionResult<ApiResponse<SanctionLetterDocumentDto>>>
        GenerateSanctionLetter(int dealId)
    {
        try
        {
            var result = await service.GenerateSanctionLetterAsync(dealId);

            return Ok(ApiResponse<SanctionLetterDocumentDto>.Ok(
                result, result.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                ApiResponse<SanctionLetterDocumentDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<SanctionLetterDocumentDto>.Fail(ex.Message));
        }
    }

    [HttpGet("{dealId:int}")]
    public async Task<ActionResult<ApiResponse<SanctionLetterDocumentDto>>>
        GetSanctionLetter(int dealId)
    {
        var result = await service.GetSanctionLetterAsync(dealId);

        if (result == null)
        {
            return NotFound(
                ApiResponse<SanctionLetterDocumentDto>.Fail(
                    "Sanction letter not found."));
        }

        return Ok(ApiResponse<SanctionLetterDocumentDto>.Ok(
            result, result.Message));
    }

    [HttpGet("{dealId:int}/download")]
    public async Task<IActionResult> Download(int dealId)
    {
        var letter = await service.GetSanctionLetterAsync(dealId);

        if (letter == null)
        {
            return NotFound(
                "Sanction letter record not found. Generate it first.");
        }

        var fileName = $"SanctionLetter_{dealId}.pdf";

        var filePath = Path.Combine(
            env.ContentRootPath,
            "wwwroot",
            "Documents",
            fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(
                "The sanction letter record exists, but its PDF file is missing. Generate the letter again.");
        }

        return PhysicalFile(filePath, "application/pdf", fileName);
    }
}
