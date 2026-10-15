using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IPaymentHistroyService
    {
        Task<List<PaymentHistroyDto>> FetchPaymentHistory(int? id);
    }
}
