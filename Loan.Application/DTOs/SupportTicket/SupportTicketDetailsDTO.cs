namespace Loan.Application.DTOs.SupportTicket
{
    public class SupportTicketDetailsDTO
    {
        public int TicketId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string LoanAccountNo { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? AttachmentPath { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? OfficerResponse { get; set; }
    }
}