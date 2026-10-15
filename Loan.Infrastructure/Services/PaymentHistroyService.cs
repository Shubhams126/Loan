using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class PaymentHistroyService : IPaymentHistroyService
    {
        AppDbContext db;
        public PaymentHistroyService(AppDbContext db)
        {
            this.db= db;
        }

        public async Task<List<PaymentHistroyDto>> FetchPaymentHistory(int? id)
        {
            var payhis = db.LoanPayments.Include(x => x.LoanAccount).Where(x => x.LoanAccountId == id)
                 .OrderByDescending(x => x.PaymentDate).Select(x => new PaymentHistroyDto
                 {
                     LoanPaymentId = x.PaymentId,
                     LoanAccountNo = x.LoanAccount!.LoanAccountNo,
                     PaymentDate = x.PaymentDate,
                     PaidDate = x.PaymentDate,
                     PaidAmount = x.PaidAmount,
                     PaymentMode = x.PaymentMode,
                     TransactionReference = x.TransactionReference,
                     PaymentStatus = x.PaymentStatus,
                     InstallmentNo = 0,
                     DueDate = x.PaymentDate,
                     Emi = x.PaidAmount
                 }).ToList();

            return payhis;
        }
    }
}
