using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface ILoanAccountService
    {
        Task<List<LoanAccountListDto>> GetAllLoanAccountsAsync();

        Task<string> CreateLoanAccount(int disbursementId);

        Task<List<LoanAccountListDto>> GetMyLoanAccounts(int customerId);

        Task<List<LoanAccountListDto>> SearchLoan(string str);
    }
}
