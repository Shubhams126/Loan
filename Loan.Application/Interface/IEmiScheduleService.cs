using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IEmiScheduleService
    {
        Task<string> GenerateEmiSchedule(int loanAccountId);

        Task<List<EmiScheduleDto>> GetEmiSchedule(int loanAccountId);
    }
}
