namespace Loan.Application.DTOs
{
    public class DashboardDTO
    {
        public int TotalCustomers { get; set; }
        public int TotalLoanCustomers { get; set; }
        public int TotalApprovedKyc { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public int TotalActiveLoans { get; set; }
        public decimal TodaysCollection { get; set; }
        public int PendingEmiCount { get; set; }
        public int ForeClosureRequests { get; set; }
    }
}

