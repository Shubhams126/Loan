namespace Loan.Application.DTOs.SupportTicket
{
    public class SupportTicketListDTO
    {
        public int TicketId { get; set; }

        public string LoanAccountNo { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}