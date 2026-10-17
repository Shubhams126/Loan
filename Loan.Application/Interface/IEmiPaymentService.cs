using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IEmiPaymentService
    {
        Task<string> MakeEmiPayment(CreateEmiPaymentDto dto, string? razorpayPaymentId = null);
       string  CreateRazorpayOrder(CreateOrderDto dto);

        Task<EmiPaymentDetailsDto?> GetEmiPaymentDetails(int id);

        Task<bool> VerifyEmiPayment(VerifyEmiPaymentDto dtos);
    }
}
