using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class EmiPaymentService : IEmiPaymentService
    {
        AppDbContext db;
        public EmiPaymentService(AppDbContext db)
        {
            this.db= db;
        }
        public async Task<string> MakeEmiPayment(CreateEmiPaymentDto dto)
        {
            decimal penalty=0,LateFees=0;
            var emi = await db.EmiSchedules.FirstOrDefaultAsync(x => x.EmiScheduleId == dto.EmiScheduleId);
            if (emi == null)
                throw new Exception("Emi not Found");

           if(emi.PaymentStatus =="Paid")
            {
                return "Emi Already Paid";
            }

            if (emi.LoanAccountId != dto.LoanAccountId)
            {
                return "EMI does not belong to this Loan Account";
            }

            var loan = await db.LoanAccounts.FirstOrDefaultAsync(x => x.LoanAccountId == dto.LoanAccountId);
            if(loan == null)
            {
                return "Loan Account Not Found";
            }
            if(loan.LoanStatus!="Active")
            {
                return "Loan is Not Active";
            }

            if (emi.DueDate < DateTime.Now && emi.PaymentStatus == "Pending")
            {
                penalty = 800;
                LateFees = 2;
            }

           decimal totalAmount = emi.Emi + penalty + LateFees;

            var loanpayment = new LoanPayment()
            {
                LoanAccountId = dto.LoanAccountId,
                PaymentName = dto.PaymentMode,
                PaymentDate = DateTime.Now,
                PaidAmount = totalAmount,
                PaymentMode = dto.PaymentMode,
                TransactionReference = Guid.NewGuid().ToString(),
                PrincipalPaid = emi.PrincipalAmount,
                InterestPaid = emi.InterestAmount,
                PenaltyPaid = penalty,
                PaymentStatus = "Success"

            };
            db.LoanPayments.Add(loanpayment);
            emi.PaymentStatus = "Paid";
            emi.PaidDate = DateTime.Now;
            loan.OutstandingPrincipal -= emi.PrincipalAmount;
            loan.TotalPaidAmount += totalAmount;
            await db.SaveChangesAsync();

            return "Emi Paid Successfully";
        }

       
    }
}
