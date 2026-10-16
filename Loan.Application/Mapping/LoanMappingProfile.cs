using AutoMapper;
using Loan.Application.DTOs;
using Loan.Domain.Models;

namespace Loan.Application.Mapping;

public sealed class LoanMappingProfile : Profile
{
    public LoanMappingProfile()
    {
        CreateMap<LoanApplyDto, LoanDeal>()
            .ForMember(d => d.DealId, o => o.Ignore())
            .ForMember(d => d.InterestRate, o => o.Ignore())
            .ForMember(d => d.EmiAmount, o => o.Ignore())
            .ForMember(d => d.ApprovedAmount, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.CurrentStatus, o => o.MapFrom(_ => "Pending"))
            .ForMember(d => d.RejectionReason, o => o.MapFrom(_ => (string?)null))
            .ForMember(d => d.AppliedDate, o => o.MapFrom(_ => DateTime.Now))
            .ForMember(d => d.Customer, o => o.Ignore())
            .ForMember(d => d.DealReviews, o => o.Ignore())
            .ForMember(d => d.SanctionLetters, o => o.Ignore())
            .ForMember(d => d.Disbursements, o => o.Ignore());

        CreateMap<LoanDeal, LoanApplyDto>()
            .ForMember(d => d.Message, o => o.MapFrom(_ => "Loan details retrieved successfully."));

        CreateMap<LoanDeal, ApprovedLoanDto>();

        CreateMap<SanctionLetter, SanctionLetterDocumentDto>()
            .ForMember(d => d.SanctionLetterId, o => o.MapFrom(s => s.SanctionId))
            .ForMember(d => d.FileName, o => o.MapFrom(s => $"SanctionLetter_{s.DealId}.pdf"))
            .ForMember(d => d.Message, o => o.MapFrom(_ => "Sanction letter retrieved successfully."));

        CreateMap<Disbursement, DisbursementDto>();

        CreateMap<LoanDeal, DisbursementDto>()
            .ForMember(d => d.DisburseAmount, o => o.MapFrom(s => s.ApprovedAmount))
            .ForMember(d => d.BankPartner, o => o.MapFrom(s => s.BankName));
    }
}
