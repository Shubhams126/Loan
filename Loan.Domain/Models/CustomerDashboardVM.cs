namespace Loan.Domain.Models
{
    public class CustomerDashboardVM
    {
        public int TotalActiveLoanAccounts { get; set; }

        public string SelectedLoanAccountNo { get; set; }

        public List<CustomerLoanDropdownVM> LoanAccounts { get; set; }

        public CustomerLoanDetailsVM SelectedLoanDetails { get; set; }
    }

    public class CustomerLoanDropdownVM
    {
        public string LoanAccountNo { get; set; }
    }

    public class CustomerLoanDetailsVM
    {
        public string LoanAccountNo { get; set; }

        public decimal OutstandingPrincipal { get; set; }

        public DateTime? NextEmiDate { get; set; }
    }
}