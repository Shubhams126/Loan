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

        Task<List<ForeClosureRequest>> GetForeClosureRequests();

        Task<string> UpdateForeClosuere(int rid, string s);

        Task<string> PayForeClosure(int rid);
    }
}
