using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class KycDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string VerificationStatus { get; set; } = "Pending";

        public string? RejectionReason { get; set; }

        public int? ReviewedByUserId { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public Customer? Customer { get; set; }
    }
}