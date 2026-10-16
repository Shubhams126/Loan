using Loan.Application.DTOs.SupportTicket;

namespace Loan.Application.Interface
{
    public interface ISupportTicketService
    {
        Task AddTicketAsync(AddSupportTicketDTO dto, int customerId);

        Task<List<SupportTicketDTO>> GetMyTicketsAsync(int customerId);

        Task<List<SupportTicketListDTO>> GetAllTicketsAsync();

        Task<SupportTicketDetailsDTO?> GetTicketByIdAsync(int ticketId);

        Task UpdateTicketAsync(int ticketId, UpdateSupportTicketDTO dto);
    }
}