using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Loan.Application.DTOs
{
    public class KycUploadDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}