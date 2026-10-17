
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class KycService : IKycService
    {
        AppDbContext db;

        public KycService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<string> UploadDocument(int customerId, string documentType, string fileName, Stream fileStream, long fileLength)
        {
            if (!await db.Customers.AnyAsync(c => c.CustomerId == customerId))
            {
                return "Customer not found.";
            }

            if (fileLength == 0)
            {
                return "Please select a file.";
            }

            if (fileLength > 5 * 1024 * 1024)
            {
                return "File must be below 5 MB.";
            }

            string extension = Path.GetExtension(fileName).ToLower();

            if (!new[] { ".pdf", ".jpg", ".jpeg", ".png" }.Contains(extension))
            {
                return "Only PDF and image files are allowed.";
            }

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "KycUploads");

            Directory.CreateDirectory(folder);

            string savedFileName = Guid.NewGuid() + extension;
            string path = Path.Combine(folder, savedFileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            KycDocument document = new KycDocument
            {
                CustomerId = customerId,
                DocumentType = documentType,
                FilePath = savedFileName,
                VerificationStatus = "Pending",
                UploadedAt = DateTime.UtcNow
            };

            db.KycDocuments.Add(document);
            await db.SaveChangesAsync();

            return "Document uploaded successfully.";
        }

        public async Task<List<KycDocumentDto>> GetCustomerDocuments(int customerId)
        {
            var data = await db.KycDocuments.Where(d => d.CustomerId == customerId)
                .Select(d => new KycDocumentDto
                {
                    DocumentId = d.DocumentId,
                    CustomerId = d.CustomerId,
                    DocumentType = d.DocumentType,
                    VerificationStatus = d.VerificationStatus,
                    RejectionReason = d.RejectionReason,
                    UploadedAt = d.UploadedAt,
                    ReviewedAt = d.ReviewedAt
                })
                .ToListAsync();
            return data;
        }

        public async Task<List<KycDocumentDto>> GetPendingDocuments()
        {
            var data = await db.KycDocuments.Where(d => d.VerificationStatus == "Pending")
                .Select(d => new KycDocumentDto
                {
                    DocumentId = d.DocumentId,
                    CustomerId = d.CustomerId,
                    DocumentType = d.DocumentType,
                    VerificationStatus = d.VerificationStatus,
                    RejectionReason = d.RejectionReason,
                    UploadedAt = d.UploadedAt,
                    ReviewedAt = d.ReviewedAt
                })
                .ToListAsync();
            return data;
        }

        public async Task<string> ApproveDocument(int documentId, int officerUserId)
        {
            var document = await db.KycDocuments.FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null)
            {
                return "Document not found.";
            }

            document.VerificationStatus = "Approved";
            document.ReviewedByUserId = officerUserId;
            document.ReviewedAt = DateTime.UtcNow;
            document.RejectionReason = null;

            await db.SaveChangesAsync();

            return "Document approved successfully.";
        }

        public async Task<string> RejectDocument(int documentId, int officerUserId, RejectKycDto dto)
        {
            var document = await db.KycDocuments.FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null)
            {
                return "Document not found.";
            }

            document.VerificationStatus = "Rejected";
            document.RejectionReason = dto.Reason;
            document.ReviewedByUserId = officerUserId;
            document.ReviewedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return "Document rejected successfully.";
        }
    }
}
