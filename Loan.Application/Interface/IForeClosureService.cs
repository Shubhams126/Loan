using Loan.Application.DTOs;
using Loan.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IForeClosureService
    {
        Task<string> CreateForeClosureRequest(CreateForeclosureDto dto);

        Task<List<ForeClosureRequestDto>> GetForeClosureRequests();
        Task<ForeClosureRequestDto> GetForeClosure(int id);


        Task<List<ForeClosureRequest>> GetForeClosureRequestbyid(int id);

        Task<string> UpdateForeClosuere(int rid, UpdateForeClosureDto dto);
        Task<string> CreateForeClosureOrder(int requestId);
        Task<bool> VerifyForeClosurePayment(VerifyForeClosurePaymentDto dto);
        Task<string> PayForeClosure(int rid);
    }
}
