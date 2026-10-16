
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Loan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KycController : ControllerBase
    {
        IKycService service;

        public KycController(IKycService service)
        {
            this.service = service;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] string documentType, /*[FromForm]*/ IFormFile file)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Please select a file.");

            using var stream = dto.File.OpenReadStream();

            var result = await service.UploadDocument(
                dto.CustomerId,
                dto.DocumentType,
                dto.File.FileName,
                stream,
                dto.File.Length
            );

            if (result != "Document uploaded successfully.")
                return BadRequest(result);

            return Ok(result);
        }
        [HttpGet("MyDocuments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyDocuments()
        {
            int customerId = int.Parse(User.FindFirst("CustomerId")!.Value);
            var documents = await service.GetCustomerDocuments(customerId);
            return Ok(documents);
        }

        [HttpGet("PendingDocuments")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> PendingDocuments()
        {
            var documents = await service.GetPendingDocuments();
            return Ok(documents);
        }

        [HttpPut("Approve/{documentId}")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> Approve(int documentId)
        {
            int officerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string result = await service.ApproveDocument(documentId, officerId);
            return Ok(new { message = result });
        }

        [HttpPut("Reject/{documentId}")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> Reject(int documentId, [FromBody] RejectKycDto dto)
        {
            int officerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            string result = await service.RejectDocument(documentId, officerId, dto);
            return Ok(new { message = result });
        }
    }
}
