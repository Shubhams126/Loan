using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class KycDocumentDto
    {
        public int DocumentId { get; set; }

        public int CustomerId { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public string VerificationStatus { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }

        public DateTime UploadedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }
}
