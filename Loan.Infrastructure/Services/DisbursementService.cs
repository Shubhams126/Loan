using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Application.Services
{
    public class DisbursementService : IDisbursementService
    {
        private readonly AppDbContext db;

        public DisbursementService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<List<DisbursementDto>> GetPendingDisbursementsAsync()
        {
            var loans = await db.LoanDeals
                .Where(x =>
                    x.CurrentStatus == "Approved" &&
                    x.ApprovedAmount > 0 &&
                    db.SanctionLetters.Any(s => s.DealId == x.DealId) &&
                    !db.Disbursements.Any(d => d.DealId == x.DealId))
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            var result = new List<DisbursementDto>();

            foreach (var loan in loans)
            {
                result.Add(new DisbursementDto
                {
                    DealId = loan.DealId,
                    DisburseAmount = loan.ApprovedAmount,
                    BankPartner = loan.BankName
                });
            }

            return result;
        }

        public async Task<DisbursementDto> DisburseLoanAsync(
            DisbursementDto dto)
        {
            if (dto.DealId <= 0)
            {
                throw new InvalidOperationException(
                    "A valid DealId is required.");
            }

            if (dto.DisburseAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Disbursement amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(dto.BankPartner))
            {
                throw new InvalidOperationException(
                    "Bank partner is required.");
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

            var alreadyDisbursed = await db.Disbursements
                .AnyAsync(x => x.DealId == dto.DealId);

            if (alreadyDisbursed)
            {
                throw new InvalidOperationException(
                    "This loan has already been disbursed.");
            }

            var accountExists = await db.LoanAccounts
                .AnyAsync(x => x.DealId == dto.DealId);

            if (accountExists)
            {
                throw new InvalidOperationException(
                    "A loan account already exists for this deal.");
            }

            var customerExists = await db.Customers
                .AnyAsync(x => x.CustomerId == loan.CustomerId);

            if (!customerExists)
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

                return new DisbursementDto
                {
                    DealId = loan.DealId,
                    DisburseAmount = loan.ApprovedAmount,
                    BankPartner = dto.BankPartner.Trim()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<DisbursementDto>> GetDisbursementsAsync()
        {
            var disbursements = await db.Disbursements
                .OrderByDescending(x => x.DisbursementDate)
                .ToListAsync();

            var result = new List<DisbursementDto>();

            foreach (var item in disbursements)
            {
                result.Add(new DisbursementDto
                {
                    DealId = item.DealId,
                    DisburseAmount = item.DisburseAmount,
                    BankPartner = item.BankPartner
                });
            }

            return result;
        }
    }
}