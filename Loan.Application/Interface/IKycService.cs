using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IKycService
    {
        Task<string> UploadDocument(int customerId, string documentType,
            string fileName, Stream fileStream, long fileLength);

        Task<List<KycDocumentDto>> GetCustomerDocuments(int customerId);

        Task<List<KycDocumentDto>> GetPendingDocuments();

        Task<string> ApproveDocument(int documentId, int officerUserId);

        Task<string> RejectDocument(int documentId, int officerUserId,
            RejectKycDto dto);

    }
}
