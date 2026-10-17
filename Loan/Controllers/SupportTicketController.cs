
using Loan.Application.DTOs.SupportTicket;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService supportTicketService;

        public SupportTicketController(
            ISupportTicketService supportTicketService)
        {
            this.supportTicketService = supportTicketService;
        }

        // API 1: Create a new support ticket.
        [HttpPost]
        public async Task<IActionResult> AddTicket(
            [FromForm] AddSupportTicketDTO dto,
            [FromQuery] int customerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (customerId <= 0)
            {
                return BadRequest("A valid customer ID is required.");
            }

            try
            {
                await supportTicketService.AddTicketAsync(dto, customerId);

                return Ok(new
                {
                    message = "Support ticket created successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // API 2: Get all tickets belonging to a customer.
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets(
            [FromQuery] int customerId)
        {
            if (customerId <= 0)
            {
                return BadRequest("A valid customer ID is required.");
            }

            var tickets =
                await supportTicketService.GetMyTicketsAsync(customerId);

            return Ok(tickets);
        }

        // API 3: Get all support tickets.
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets =
                await supportTicketService.GetAllTicketsAsync();

            return Ok(tickets);
        }

        // API 4: Get details of a particular ticket.
        [HttpGet("{ticketId:int}")]
        public async Task<IActionResult> GetTicketById(int ticketId)
        {
            if (ticketId <= 0)
            {
                return BadRequest("A valid ticket ID is required.");
            }

            var ticket =
                await supportTicketService.GetTicketByIdAsync(ticketId);

            if (ticket == null)
            {
                return NotFound("Support ticket not found.");
            }

            return Ok(ticket);
        }

        // API 5: Update ticket status and officer response.
        [HttpPut("{ticketId:int}")]
        public async Task<IActionResult> UpdateTicket(
            int ticketId,
            [FromBody] UpdateSupportTicketDTO dto)
        {
            if (ticketId <= 0)
            {
                return BadRequest("A valid ticket ID is required.");
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest("Ticket status is required.");
            }

            var allowedStatuses = new[]
            {
                "Pending",
                "InProgress",
                "Resolved"
            };

            if (!allowedStatuses.Contains(dto.Status))
            {
                return BadRequest(
                    "Status must be Pending, InProgress, or Resolved.");
            }

            try
            {
                await supportTicketService.UpdateTicketAsync(ticketId, dto);

                return Ok(new
                {
                    message = "Support ticket updated successfully."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Support ticket not found.")
                {
                    return NotFound(ex.Message);
                }

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}