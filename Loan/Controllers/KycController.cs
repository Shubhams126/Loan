
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

        [HttpPost("Upload")]
        [Authorize(Roles = "Customer")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] string documentType, /*[FromForm]*/ IFormFile file)
        {
            int customerId = int.Parse(User.FindFirst("CustomerId")!.Value);
            using var stream = file.OpenReadStream();
            string result = await service.UploadDocument(customerId, documentType, file.FileName, stream, file.Length);
            return Ok(new { message = result });
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
