
using AutoMapper;
using Loan.Application.DTOs.SupportTicket;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;

        public SupportTicketService(AppDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task AddTicketAsync(AddSupportTicketDTO dto, int customerId)
        {
            var loanAccount = await db.LoanAccounts
                .FirstOrDefaultAsync(x =>
                    x.CustomerId == customerId && x.Status == "Active");

            if (loanAccount == null)
            {
                throw new Exception(
                    "No active loan account found for this customer.");
            }

            string? attachmentPath = null;

            if (dto.Attachment != null)
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(dto.Attachment.FileName);

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(
                    filePath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }

                attachmentPath = "/uploads/" + fileName;
            }

            var ticket = new SupportTicket
            {
                CustomerId = customerId,
                LoanAccountId = loanAccount.LoanAccountId,
                Subject = dto.Subject,
                Description = dto.Description,
                AttachmentPath = attachmentPath,
                CreatedDate = DateTime.Now,
                Status = "Pending",
                OfficerResponse = null
            };

            db.SupportTickets.Add(ticket);
            await db.SaveChangesAsync();
        }

        public async Task<List<SupportTicketListDTO>> GetAllTicketsAsync()
        {
            var tickets = await db.SupportTickets
                .Include(x => x.Customer)
                .Include(x => x.LoanAccount)
                .ToListAsync();

            return mapper.Map<List<SupportTicketListDTO>>(tickets);
        }

        public async Task<List<SupportTicketDTO>> GetMyTicketsAsync(
            int customerId)
        {
            var tickets = await db.SupportTickets
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();

            return mapper.Map<List<SupportTicketDTO>>(tickets);
        }

        public async Task<SupportTicketDetailsDTO?> GetTicketByIdAsync(
            int ticketId)
        {
            var ticket = await db.SupportTickets
                .Include(x => x.Customer)
                .Include(x => x.LoanAccount)
                .FirstOrDefaultAsync(x => x.TicketId == ticketId);

            if (ticket == null)
            {
                return null;
            }

            return mapper.Map<SupportTicketDetailsDTO>(ticket);
        }

        public async Task UpdateTicketAsync(
            int ticketId,
            UpdateSupportTicketDTO dto)
        {
            var ticket = await db.SupportTickets
                .FirstOrDefaultAsync(x => x.TicketId == ticketId);

            if (ticket == null)
            {
                throw new Exception("Support ticket not found.");
            }

            ticket.Status = dto.Status;
            ticket.OfficerResponse = dto.OfficerResponse;

            await db.SaveChangesAsync();
        }
    }
}