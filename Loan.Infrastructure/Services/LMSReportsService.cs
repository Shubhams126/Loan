using Loan.Application.DTOs.Reports;
using Loan.Application.Interface;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class LMSReportsService : ILMSReportsService
    {
        private readonly AppDbContext db;

        public LMSReportsService(AppDbContext db)
        {
            this.db = db;
        }

        // 1. LMS Dashboard Report
        public async Task<LMSReportsDashboardDTO> GetDashboardAsync(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var loans = await db.LoanAccounts
                .Include(x => x.LoanDeal)
                .ToListAsync();

            var dashboard = new LMSReportsDashboardDTO();

            dashboard.Summary.TotalLoans = loans.Count;

            dashboard.Summary.ActiveLoans =
                loans.Count(x => x.LoanStatus == "Active");

            dashboard.Summary.ClosedLoans =
                loans.Count(x => x.LoanStatus == "Closed");

            var totalOutstanding =
                loans.Sum(x => x.OutstandingPrincipal);

            dashboard.LoanPortfolioChart = loans
                .GroupBy(x => x.LoanDeal?.LoanType ?? "Unknown")
                .Select(g => new LoanPortfolioChartDTO
                {
                    Product = g.Key,

                    OutstandingAmount =
                        g.Sum(x => x.OutstandingPrincipal),

                    Percentage = totalOutstanding > 0
                        ? Math.Round(
                            g.Sum(x => x.OutstandingPrincipal)
                            / totalOutstanding * 100, 2)
                        : 0
                })
                .ToList();

            var collectionQuery = db.EmiSchedules.AsQueryable();

            if (fromDate.HasValue)
            {
                collectionQuery = collectionQuery.Where(
                    x => x.DueDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                collectionQuery = collectionQuery.Where(
                    x => x.DueDate < toDate.Value.Date.AddDays(1));
            }

            var emis = await collectionQuery.ToListAsync();

            dashboard.CollectionEfficiencyChart = emis
                .GroupBy(x => new
                {
                    x.DueDate.Year,
                    x.DueDate.Month
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var billed = g.Sum(x => x.Emi);

                    var collected = g
                        .Where(x => x.PaymentStatus == "Paid")
                        .Sum(x => x.Emi);

                    return new CollectionEfficiencyChartDTO
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",

                        CollectionPercentage = billed > 0
                            ? Math.Round(collected / billed * 100, 2)
                            : 0
                    };
                })
                .ToList();

            return dashboard;
        }

        // 2. Loan Portfolio Report
        public async Task<List<LoanPortfolioReportDTO>>
            GetLoanPortfolioReportAsync()
        {
            var loans = await db.LoanAccounts
                .Include(x => x.LoanDeal)
                .Where(x => x.LoanStatus == "Active")
                .ToListAsync();

            var totalOutstanding =
                loans.Sum(x => x.OutstandingPrincipal);

            return loans
                .GroupBy(x => x.LoanDeal?.LoanType ?? "Unknown")
                .Select(g => new LoanPortfolioReportDTO
                {
                    Product = g.Key,

                    ActiveLoans = g.Count(),

                    OutstandingAmount =
                        g.Sum(x => x.OutstandingPrincipal),

                    Percentage = totalOutstanding > 0
                        ? Math.Round(
                            g.Sum(x => x.OutstandingPrincipal)
                            / totalOutstanding * 100, 2)
                        : 0
                })
                .ToList();
        }

        // 3. EMI Collection Report
        public async Task<List<EMICollectionReportDTO>>
            GetEMICollectionReportAsync(
                DateTime? fromDate,
                DateTime? toDate)
        {
            var query = db.EmiSchedules.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(
                    x => x.DueDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(
                    x => x.DueDate < toDate.Value.Date.AddDays(1));
            }

            var emis = await query.ToListAsync();

            var totalBilled = emis.Sum(x => x.Emi);

            var paidEmis = emis
                .Where(x => x.PaymentStatus == "Paid")
                .ToList();

            var pendingEmis = emis
                .Where(x => x.PaymentStatus == "Pending")
                .ToList();

            var totalCollected = paidEmis.Sum(x => x.Emi);
            var totalPending = pendingEmis.Sum(x => x.Emi);

            return new List<EMICollectionReportDTO>
            {
                new()
                {
                    Particulars = "EMI Billed",
                    Count = emis.Count,
                    Amount = totalBilled,
                    Percentage = totalBilled > 0 ? 100 : 0
                },

                new()
                {
                    Particulars = "EMI Collected",
                    Count = paidEmis.Count,
                    Amount = totalCollected,
                    Percentage = totalBilled > 0
                        ? Math.Round(
                            totalCollected / totalBilled * 100, 2)
                        : 0
                },

                new()
                {
                    Particulars = "EMI Pending",
                    Count = pendingEmis.Count,
                    Amount = totalPending,
                    Percentage = totalBilled > 0
                        ? Math.Round(
                            totalPending / totalBilled * 100, 2)
                        : 0
                }
            };
        }

        // 4. Overdue Report
        public async Task<List<OverdueReportDTO>>
            GetOverdueReportAsync(DateTime? asOnDate)
        {
            var reportDate = (asOnDate ?? DateTime.Today).Date;

            var overdueEmis = await db.EmiSchedules
                .Where(x =>
                    x.PaymentStatus != "Paid"
                    && x.DueDate < reportDate)
                .ToListAsync();

            return overdueEmis
                .GroupBy(x =>
                {
                    var days = (reportDate - x.DueDate.Date).Days;

                    if (days <= 30)
                        return "1 - 30 Days";

                    if (days <= 60)
                        return "31 - 60 Days";

                    if (days <= 90)
                        return "61 - 90 Days";

                    return "Above 90 Days";
                })
                .Select(g => new OverdueReportDTO
                {
                    DaysOverdue = g.Key,

                    Accounts = g
                        .Select(x => x.LoanAccountId)
                        .Distinct()
                        .Count(),

                    OutstandingAmount = g.Sum(x => x.Emi)
                })
                .OrderBy(x =>
                    x.DaysOverdue == "1 - 30 Days" ? 1 :
                    x.DaysOverdue == "31 - 60 Days" ? 2 :
                    x.DaysOverdue == "61 - 90 Days" ? 3 : 4)
                .ToList();
        }

        // 5. Disbursement Report
        public async Task<List<DisbursementReportDTO>>
            GetDisbursementReportAsync(
                DateTime? fromDate,
                DateTime? toDate)
        {
            var query = db.Disbursements
                .Where(x => x.Status == "Completed")
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(
                    x => x.DisbursementDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(
                    x => x.DisbursementDate < toDate.Value.Date.AddDays(1));
            }

            var disbursements = await query.ToListAsync();

            return disbursements
                .GroupBy(x => new
                {
                    x.DisbursementDate.Year,
                    x.DisbursementDate.Month
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new DisbursementReportDTO
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",

                    LoansDisbursed = g.Count(),

                    DisbursementAmount =
                        g.Sum(x => x.DisburseAmount)
                })
                .ToList();
        }

        // 6. Prepayment Report
        public async Task<List<PrepaymentReportDTO>>
            GetPrepaymentReportAsync(
                DateTime? fromDate,
                DateTime? toDate)
        {
            var query = db.LoanPayments
                .Where(x => x.PaymentStatus == "Success")
                .Where(x =>
                    x.PaymentName == "Part Payment"
                    || x.PaymentName == "Foreclosure")
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(
                    x => x.PaymentDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(
                    x => x.PaymentDate < toDate.Value.Date.AddDays(1));
            }

            var payments = await query.ToListAsync();

            return payments
                .GroupBy(x => x.PaymentName)
                .Select(g => new PrepaymentReportDTO
                {
                    Type = g.Key,

                    Accounts = g
                        .Select(x => x.LoanAccountId)
                        .Distinct()
                        .Count(),

                    PrepaymentAmount = g.Sum(x => x.PaidAmount)
                })
                .ToList();
        }

        // Compatibility method for the interface/controller.
        // Reuses the existing dashboard implementation.
        public Task<LMSReportsDashboardDTO> GetLMSReportsDashboardAsync(
            DateTime? fromDate,
            DateTime? toDate)
        {
            return GetDashboardAsync(fromDate, toDate);
        }
    }
}