using AutoMapper;
using Loan.Application.DTOs.SupportTicket;
using Loan.Domain.Models;

namespace Loan.Application.Mapper
{
    public class SupportTicketMappingProfile : Profile
    {
        public SupportTicketMappingProfile()
        {
            // Map ticket entity to the basic ticket DTO.
            CreateMap<SupportTicket, SupportTicketDTO>();


        // Map ticket entity to the ticket list DTO.
        CreateMap<SupportTicket, SupportTicketListDTO>();

            // Map ticket entity to the full details DTO.
            CreateMap<SupportTicket, SupportTicketDetailsDTO>();

            // Map the add-ticket DTO to the entity.
            // AttachmentPath is handled separately in the service.
            CreateMap<AddSupportTicketDTO, SupportTicket>()
                .ForMember(
                    destination => destination.AttachmentPath,
                    options => options.Ignore()
                );
        }
    }


}
