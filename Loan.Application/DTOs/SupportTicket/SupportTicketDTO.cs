namespace Loan.Application.DTOs.SupportTicket
{
    public class SupportTicketDTO
    {
        public int TicketId { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? AttachmentPath { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? OfficerResponse { get; set; }
    }
}