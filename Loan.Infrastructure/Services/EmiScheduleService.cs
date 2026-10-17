
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class EmiScheduleService : IEmiScheduleService
    {
        private readonly AppDbContext db;

        public EmiScheduleService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<string> GenerateEmiSchedule(int loanAccountId)
        {
            var loan = await db.LoanAccounts
                .FirstOrDefaultAsync(x => x.LoanAccountId == loanAccountId);

            if (loan == null)
            {
                return "Loan account not found";
            }

            var existingSchedule = await db.EmiSchedules
                .AnyAsync(x => x.LoanAccountId == loanAccountId);

            if (existingSchedule)
            {
                return "EMI schedule already exists";
            }

            decimal openingBalance = loan.LoanAmount;
            decimal monthlyRate = loan.InterestRate / 12 / 100;

            for (int i = 1; i <= loan.TenureMonths; i++)
            {
                decimal interest = Math.Round(openingBalance * monthlyRate, 2);
                decimal principal = loan.EmiAmount - interest;

                // Adjust the final installment to clear the balance.
                if (i == loan.TenureMonths || principal > openingBalance)
                {
                    principal = openingBalance;
                }

                decimal emi = principal + interest;
                decimal closingBalance = openingBalance - principal;

                EmiSchedule schedule = new EmiSchedule
                {
                    LoanAccountId = loan.LoanAccountId,
                    InstallmentNo = i,
                    DueDate = loan.DisbursementDate.AddMonths(i),
                    Emi = emi,
                    PrincipalAmount = principal,
                    InterestAmount = interest,
                    OpeningBalance = openingBalance,
                    ClosingBalance = closingBalance,
                    PaymentStatus = "Pending",
                    PaidDate = null
                };

                db.EmiSchedules.Add(schedule);

                openingBalance = closingBalance;
            }

            await db.SaveChangesAsync();

            return "EMI schedule generated successfully";
        }


        public async Task<List<EmiScheduleDto>> GetEmiSchedule(int loanAccountId)
        {
            return await db.EmiSchedules
                .Where(x => x.LoanAccountId == loanAccountId)
                .OrderBy(x => x.InstallmentNo)
                .Select(x => new EmiScheduleDto
                {
                    EmiScheduleId = x.EmiScheduleId,
                    InstallmentNo = x.InstallmentNo,
                    DueDate = x.DueDate,
                    Emi = x.Emi,
                    PrincipalAmount = x.PrincipalAmount,
                    InterestAmount = x.InterestAmount,
                    OpeningBalance = x.OpeningBalance,
                    ClosingBalance = x.ClosingBalance,
                    PaymentStatus = x.PaymentStatus
                })
                .ToListAsync();
        }
    }
}
