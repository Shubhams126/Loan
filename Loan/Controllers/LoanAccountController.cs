using Loan.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/loan-accounts")]
public class LoanAccountController : ControllerBase
{
    ILoanAccountService Service;

    public LoanAccountController(ILoanAccountService Service)
    {
        this.Service = Service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLoanAccounts()
    {
        var result = await Service.GetAllLoanAccountsAsync();

        return Ok(result);
    }

    [HttpPost("create/{disbursementId}")]
    public async Task<IActionResult> CreateLoanAccount(int disbursementId)
    {
        var result = await Service.CreateLoanAccount(disbursementId);

        if (result == "Loan account created successfully")
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [Authorize]
    [HttpGet("my-accounts")]
    public async Task<IActionResult> GetMyLoanAccounts()
    {
        var allClaims = User.Claims
        .Select(c => new { c.Type, c.Value })
        .ToList();

        var cstmClaimId = User.FindFirst("CustomerId");
        if (cstmClaimId == null)
        {
            return Unauthorized("CustomerId not found In Token");
        }

        int cstmId = int.Parse(cstmClaimId.Value);


        var result = await Service.GetMyLoanAccounts(cstmId);
        return Ok(result);
    }
}