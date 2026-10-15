
using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetApprovedLoans()
        {
            var data = await service.GetApprovedLoansAsync();

            return Ok(new
            {
                Success = true,
                Data = data
            });
        }

        [HttpPost("{dealId:int}/generate")]
        public async Task<IActionResult> GenerateSanctionLetter(
            int dealId)
        {
            try
            {
                var result =
                    await service.GenerateSanctionLetterAsync(dealId);

                return Ok(new
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{dealId:int}")]
        public async Task<IActionResult> GetSanctionLetter(int dealId)
        {
            var result =
                await service.GetSanctionLetterAsync(dealId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Sanction letter not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Data = result
            });
        }

        [HttpGet("{dealId:int}/download")]
        public async Task<IActionResult> DownloadSanctionLetter(
            int dealId)
        {
            var result =
                await service.GetSanctionLetterAsync(dealId);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Sanction letter not found."
                });
            }

            var filePath = Path.Combine(
                env.ContentRootPath,
                "wwwroot",
                "Documents",
                result.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Sanction letter PDF file not found."
                });
            }

            return PhysicalFile(
                filePath,
                "application/pdf",
                result.FileName);
        }
    }
}
