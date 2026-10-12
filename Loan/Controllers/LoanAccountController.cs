using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/loan-accounts")]
public class LoanAccountController : ControllerBase
{
    private readonly ILoanAccountService Service;

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
}