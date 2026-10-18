
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Loan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KycController : ControllerBase
    {
        private readonly IKycService service;

        public KycController(IKycService service)
        {
            this.service = service;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            [FromForm] string documentType, IFormFile file)
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;

            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return Unauthorized(new
                {
                    message = "Customer ID not found in token."
                });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a file."
                });
            }

            using var stream = file.OpenReadStream();

            string result = await service.UploadDocument(
                customerId,
                documentType,
                file.FileName,
                stream,
                file.Length
            );

            if (result != "Document uploaded successfully.")
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        [HttpGet("MyDocuments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyDocuments()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;

            if (!int.TryParse(customerIdClaim, out int customerId))
            {
                return Unauthorized(new
                {
                    message = "Customer ID not found in token."
                });
            }

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
            var officerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(officerClaim, out int officerId))
            {
                return Unauthorized(new
                {
                    message = "Officer ID not found in token."
                });
            }

            string result = await service.ApproveDocument(
                documentId,
                officerId
            );

            return Ok(new { message = result });
        }

        [HttpPut("Reject/{documentId}")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> Reject(
            int documentId,
            [FromBody] RejectKycDto dto)
        {
            var officerClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(officerClaim, out int officerId))
            {
                return Unauthorized(new
                {
                    message = "Officer ID not found in token."
                });
            }

            string result = await service.RejectDocument(
                documentId,
                officerId,
                dto
            );

            return Ok(new { message = result });
        }
    }
}
