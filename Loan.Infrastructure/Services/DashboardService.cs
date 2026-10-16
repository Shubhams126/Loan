using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Infrastructure.Data;

namespace Loan.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext db;

        public DashboardService(AppDbContext db)
        {
            this.db = db;
        }

        public DashboardDTO GetDashboardData()
        {
            var today = DateTime.Today;

            var dashboard = new DashboardDTO();

            dashboard.TotalCustomers = db.Customers.Count();

            dashboard.TotalLoanCustomers = db.LoanAccounts
                .Select(x => x.CustomerId)
                .Distinct()
                .Count();

            dashboard.TotalApprovedKyc = db.KycDocuments
                .Count(x => x.Status == "Approved");

            dashboard.TotalLoanAmount = db.LoanAccounts
                .Sum(x => (decimal?)x.LoanAmount) ?? 0;

            dashboard.TotalActiveLoans = db.LoanAccounts
                .Count(x => x.Status == "Active");

            dashboard.TodaysCollection = db.LoanPayments
                .Where(x => x.PaymentDate.Date == today)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            dashboard.PendingEmiCount = db.EmiSchedules
                .Count(x => x.Status == "Pending");

            dashboard.ForeClosureRequests = db.ForeClosureRequests
                .Count(x => x.Status == "Pending");

            return dashboard;
        }
    }
}

