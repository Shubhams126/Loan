
using AutoMapper;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class LoanDealService : ILoanDealService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;

        public LoanDealService(AppDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<PagedResult<LoanApplyDto>>
            GetPendingLoansAsync(int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.LoanDeals
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(x =>
                    x.CurrentStatus == "Pending" ||
                    x.CurrentStatus == "Manual Review");

            var totalCount = await query.CountAsync();

            var loans = await query
                .OrderByDescending(x => x.AppliedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = mapper.Map<List<LoanApplyDto>>(loans);

            for (int i = 0; i < loans.Count; i++)
            {
                var loan = loans[i];
                var customer = loan.Customer;

                items[i].CustomerId = loan.CustomerId;
                items[i].CustomerName = customer == null
                    ? string.Empty
                    : $"{customer.FirstName} {customer.LastName}".Trim();

                items[i].MonthlyIncome = customer?.MonthlyIncome ?? 0;
                items[i].BankName = loan.BankName;
            }

            return PagedResult<LoanApplyDto>.Create(
                items, page, pageSize, totalCount);
        }

        public async Task<LoanApplyDto?> GetLoanByIdAsync(int dealId)
        {
            var loan = await db.LoanDeals
                .AsNoTracking()
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null)
            {
                return null;
            }

            var result = mapper.Map<LoanApplyDto>(loan);

            result.CustomerId = loan.CustomerId;
            result.CustomerName = loan.Customer == null
                ? string.Empty
                : $"{loan.Customer.FirstName} {loan.Customer.LastName}".Trim();

            result.MonthlyIncome = loan.Customer?.MonthlyIncome ?? 0;
            result.BankName = loan.BankName;

            return result;
        }

        public async Task<string> EvaluateCibilAsync(int dealId)
        {
            var loan = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null)
            {
                return "Loan deal not found.";
            }

            if (loan.CurrentStatus != "Pending")
            {
                return $"Cannot evaluate this loan because its status is {loan.CurrentStatus}.";
            }

            var cibilReport = await db.CibilReports
                .AsNoTracking()
                .Where(x => x.CustomerId == loan.CustomerId)
                .OrderByDescending(x => x.CheckDate)
                .FirstOrDefaultAsync();

            if (cibilReport == null)
            {
                loan.CurrentStatus = "Manual Review";
                await db.SaveChangesAsync();

                return "No CIBIL report found. Loan sent for manual review.";
            }

            int score = cibilReport.CibilScore;

            if (score > 800)
            {
                loan.CurrentStatus = "Approved";
                loan.ApprovedAmount = loan.LoanAmount;
                loan.RejectionReason = null;

                loan.EmiAmount = CalculateEmi(
                    loan.ApprovedAmount,
                    loan.InterestRate,
                    loan.TenureMonths);

                await db.SaveChangesAsync();

                return $"Loan automatically approved. CIBIL score: {score}.";
            }
            else if (score >= 650 && score <= 799)
            {
                loan.CurrentStatus = "Manual Review";

                await db.SaveChangesAsync();

                return $"Loan sent for manual review. CIBIL score: {score}.";
            }
            else
            {
                loan.CurrentStatus = "Rejected";
                loan.ApprovedAmount = 0;
                loan.RejectionReason =
                    $"Loan automatically rejected due to low CIBIL score: {score}.";

                await db.SaveChangesAsync();

                return $"Loan automatically rejected. CIBIL score: {score}.";
            }
        }


        public async Task<bool> ApproveLoanAsync(
            int dealId,
            LoanDealReviewDto dto)
        {
            var loan = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null ||
                (loan.CurrentStatus != "Pending" &&
                 loan.CurrentStatus != "Manual Review"))
            {
                return false;
            }

            if (dto.ApprovedAmount <= 0 ||
                dto.ApprovedAmount > loan.LoanAmount)
            {
                return false;
            }

            loan.ApprovedAmount = dto.ApprovedAmount;
            loan.CurrentStatus = "Approved";
            loan.RejectionReason = null;

            loan.EmiAmount = CalculateEmi(
                dto.ApprovedAmount,
                loan.InterestRate,
                loan.TenureMonths);

            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectLoanAsync(
            int dealId,
            LoanDealReviewDto dto)
        {
            var loan = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null ||
                (loan.CurrentStatus != "Pending" &&
                 loan.CurrentStatus != "Manual Review") ||
                string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                return false;
            }

            loan.CurrentStatus = "Rejected";
            loan.ApprovedAmount = 0;
            loan.RejectionReason = dto.RejectionReason.Trim();

            await db.SaveChangesAsync();

            return true;
        }


        private static decimal CalculateEmi(
            decimal amount,
            double annualRate,
            int months)
        {
            var monthlyRate = annualRate / 12 / 100;

            if (monthlyRate == 0)
            {
                return Math.Round(amount / months, 2);
            }

            var factor = Math.Pow(1 + monthlyRate, months);

            return Math.Round(
                (decimal)(
                    (double)amount * monthlyRate * factor /
                    (factor - 1)),
                2);
        }
    }
}
