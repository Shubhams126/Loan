using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


public class LoanAccountService : ILoanAccountService
{
    private readonly AppDbContext db;

    public LoanAccountService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<string> CreateLoanAccount(int disbursementId)
    {
        var disbursment = await db.Disbursements.FirstOrDefaultAsync(x => x.DisbursementId == disbursementId);

        if(disbursment == null)
        {
            return "Disbursment Not Found";
        }
        if (disbursment.Status == "Disbursed")
        {
            var deal = await db.LoanDeals.FirstOrDefaultAsync(x => x.DealId == disbursment.DealId);

            if(deal == null)
            {
                return "Lone Deal Not Found";
            }

            var ExistingLoneAccount = await db.LoanAccounts.FirstOrDefaultAsync(x => x.DealId == deal.DealId);

            if(ExistingLoneAccount != null)
            {
                return "Lone Account Already Exist";
            }

            LoanAccount account = new LoanAccount();

            account.DealId = deal.DealId;
            account.CustomerId = deal.CustomerId;
            account.LoanAccountNo = "LA" + deal.DealId;
            account.LoanAmount = deal.ApprovedAmount;
            account.OutstandingPrincipal = deal.ApprovedAmount;
            account.LoanStatus = "Active";
            account.InterestRate = Convert.ToDecimal(deal.InterestRate);
            account.TenureMonths = deal.TenureMonths;
            account.EmiAmount = deal.EmiAmount;
            account.DisbursementDate = disbursment.DisbursementDate;
            account.TotalPaidAmount = 0;
            account.CreatedAt = DateTime.Now;

            db.LoanAccounts.AddAsync(account);
            await db.SaveChangesAsync();

            return "Loane Account Created Successfully";
        }
        else
        {
            return "Loan account cannot be created because status is not Approved";

        }
    }

    public async Task<List<LoanAccountListDto>> GetAllLoanAccountsAsync()
    {
        var loanAccounts = await db.LoanAccounts
            .Select(x => new LoanAccountListDto
            {
                LoanAccountId = x.LoanAccountId,
                LoanAccountNo = x.LoanAccountNo,
                CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                LoanAmount = x.LoanAmount,
                OutstandingPrincipal = x.OutstandingPrincipal,
                Status = x.LoanStatus,
                InterestRate = x.InterestRate,
                TenureMonths = x.TenureMonths,
                EmiAmount = x.EmiAmount,
                DisbursementDate = x.DisbursementDate,
                TotalPaid = x.TotalPaidAmount
            })
            .ToListAsync();

        return loanAccounts;
    }

    public async Task<List<LoanAccountListDto>> GetMyLoanAccounts(int customerId)
    {
        var accounts = await db.LoanAccounts.Where(a => a.CustomerId == customerId).Select(a => new LoanAccountListDto
        {
            LoanAccountId = a.LoanAccountId,
            LoanAccountNo = a.LoanAccountNo,
            CustomerName = a.Customer != null ? a.Customer.FirstName + " " + a.Customer.LastName :"",
            LoanAmount= a.LoanAmount,
            OutstandingPrincipal = a.OutstandingPrincipal,
            Status = a.LoanStatus,
            InterestRate = a.InterestRate,
            TenureMonths = a.TenureMonths,
            EmiAmount = a.EmiAmount,
            DisbursementDate = a.DisbursementDate,
            TotalPaid = a.TotalPaidAmount
        }).ToListAsync();
        return accounts;
    }

    public Task<List<LoanAccountListDto>> SearchLoan(string str)
    {
        throw new NotImplementedException();
    }
}