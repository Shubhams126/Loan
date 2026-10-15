using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisbursementController : ControllerBase
    {
        private readonly IDisbursementService service;

        public DisbursementController(IDisbursementService service)
        {
            this.service = service;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingDisbursements()
        {
            var result = await service.GetPendingDisbursementsAsync();
            return Ok(result);
        }

        [HttpPost("disburse")]
        public async Task<IActionResult> DisburseLoan(
            [FromBody] DisbursementDto dto)
        {
            try
            {
                var result = await service.DisburseLoanAsync(dto);

                return Ok(new
                {
                    Message = "Loan disbursed successfully. Loan status is now Active.",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDisbursements()
        {
            var result = await service.GetDisbursementsAsync();
            return Ok(result);
        }
    }
}