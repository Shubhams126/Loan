
using Microsoft.AspNetCore.Http;

namespace Loan.Application.DTOs.SupportTicket
{
    public class AddSupportTicketDTO
    {
        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public IFormFile? Attachment { get; set; }
    }
}