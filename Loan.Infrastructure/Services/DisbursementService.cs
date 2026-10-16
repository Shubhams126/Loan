
using AutoMapper;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class DisbursementService : IDisbursementService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;

        public DisbursementService(AppDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<PagedResult<DisbursementDto>>
            GetPendingDisbursementsAsync(int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.LoanDeals
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(x =>
                    x.CurrentStatus == "Approved" &&
                    x.ApprovedAmount > 0 &&
                    db.SanctionLetters.Any(s =>
                        s.DealId == x.DealId) &&
                    !db.Disbursements.Any(d =>
                        d.DealId == x.DealId) &&
                    !db.LoanAccounts.Any(a =>
                        a.DealId == x.DealId));

            var totalCount = await query.CountAsync();

            var loans = await query
                .OrderByDescending(x => x.AppliedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = mapper.Map<List<DisbursementDto>>(loans);

            for (int i = 0; i < loans.Count; i++)
            {
                var loan = loans[i];
                var customer = loan.Customer;

                items[i].DealId = loan.DealId;
                items[i].DisburseAmount = loan.ApprovedAmount;
                items[i].CustomerId = loan.CustomerId;
                items[i].CustomerName = customer == null
                    ? string.Empty
                    : $"{customer.FirstName} {customer.LastName}".Trim();

                items[i].MonthlyIncome = customer?.MonthlyIncome ?? 0;
                items[i].BankName = loan.BankName;
            }

            return PagedResult<DisbursementDto>.Create(
                items, page, pageSize, totalCount);
        }

        public async Task<DisbursementDto> DisburseLoanAsync(
            DisbursementDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.DealId <= 0)
            {
                throw new InvalidOperationException(
                    "A valid Deal ID is required.");
            }

            if (dto.DisburseAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Disbursement amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(dto.BankPartner))
            {
                throw new InvalidOperationException(
                    "Please select a bank partner.");
            }

            var loan = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dto.DealId);

            if (loan == null)
            {
                throw new KeyNotFoundException("Loan deal not found.");
            }

            if (loan.CurrentStatus != "Approved")
            {
                throw new InvalidOperationException(
                    "Only approved loans can be disbursed.");
            }

            if (loan.ApprovedAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Approved loan amount must be greater than zero.");
            }

            if (dto.DisburseAmount != loan.ApprovedAmount)
            {
                throw new InvalidOperationException(
                    $"Disbursement amount must equal the approved amount: {loan.ApprovedAmount}.");
            }

            var sanctionExists = await db.SanctionLetters
                .AnyAsync(x => x.DealId == dto.DealId);

            if (!sanctionExists)
            {
                throw new InvalidOperationException(
                    "Generate the sanction letter before disbursing the loan.");
            }

            if (await db.Disbursements.AnyAsync(
                    x => x.DealId == dto.DealId))
            {
                throw new InvalidOperationException(
                    "This loan has already been disbursed.");
            }

            if (await db.LoanAccounts.AnyAsync(
                    x => x.DealId == dto.DealId))
            {
                throw new InvalidOperationException(
                    "A loan account already exists for this deal.");
            }

            var customer = await db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerId == loan.CustomerId);

            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            var disbursementDate = DateTime.Now;

            await using var transaction =
                await db.Database.BeginTransactionAsync();

            try
            {
                var disbursement = new Disbursement
                {
                    DealId = loan.DealId,
                    DisburseAmount = loan.ApprovedAmount,
                    BankPartner = dto.BankPartner.Trim(),
                    DisbursementDate = disbursementDate,
                    Status = "Completed"
                };

                db.Disbursements.Add(disbursement);

                var account = new LoanAccount
                {
                    DealId = loan.DealId,
                    CustomerId = loan.CustomerId,
                    LoanAccountNo =
                        $"LN{disbursementDate:yyyyMMdd}{loan.DealId:D6}",
                    LoanAmount = loan.ApprovedAmount,
                    OutstandingPrincipal = loan.ApprovedAmount,
                    LoanStatus = "Active",
                    InterestRate = Convert.ToDecimal(loan.InterestRate),
                    TenureMonths = loan.TenureMonths,
                    EmiAmount = loan.EmiAmount,
                    DisbursementDate = disbursementDate,
                    TotalPaidAmount = 0m,
                    CreatedAt = disbursementDate
                };

                db.LoanAccounts.Add(account);

                loan.CurrentStatus = "Active";

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = mapper.Map<DisbursementDto>(disbursement);

                result.DealId = loan.DealId;
                result.DisburseAmount = loan.ApprovedAmount;
                result.CustomerId = customer.CustomerId;
                result.CustomerName =
                    $"{customer.FirstName} {customer.LastName}".Trim();
                result.MonthlyIncome = customer.MonthlyIncome;
                result.BankName = loan.BankName;

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PagedResult<DisbursementDto>>
            GetDisbursementsAsync(int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.Disbursements.AsNoTracking();

            var totalCount = await query.CountAsync();

            var rows = await query
                .OrderByDescending(x => x.DisbursementDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = mapper.Map<List<DisbursementDto>>(rows);

            // Completed disbursement rows need their loan/customer details.
            foreach (var item in items)
            {
                var loan = await db.LoanDeals
                    .AsNoTracking()
                    .Include(x => x.Customer)
                    .FirstOrDefaultAsync(x => x.DealId == item.DealId);

                if (loan != null)
                {
                    item.CustomerId = loan.CustomerId;
                    item.CustomerName = loan.Customer == null
                        ? string.Empty
                        : $"{loan.Customer.FirstName} {loan.Customer.LastName}".Trim();

                    item.MonthlyIncome = loan.Customer?.MonthlyIncome ?? 0;
                    item.BankName = loan.BankName;
                }
            }

            return PagedResult<DisbursementDto>.Create(
                items, page, pageSize, totalCount);
        }
    }
}
