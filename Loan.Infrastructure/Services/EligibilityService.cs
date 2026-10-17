using Loan.Application.Interface;
using Loan.Application.DTOs;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class EligibilityService : IEligibilityService
    {
        AppDbContext db;
        const decimal FoirPercent = 40;   // total monthly obligation should be 40% of monthly income

        public EligibilityService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<EligibilityResult?> CheckEligibility(int customerId, int tenureMonths)
        {
            if (GetTenureRate(tenureMonths) == 0)
            {
                return null;
            }

            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return null;
            }

            var report = await db.CibilReports
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CheckDate)
                .FirstOrDefaultAsync();

            if (report == null)
            {
                return null;
            }

            var previousResult = await db.EligibilityResults
                .Include(e => e.Customer)
                .Where(e => e.CustomerId == customerId)
                .OrderByDescending(e => e.EligibilityId)
                .FirstOrDefaultAsync();

            if (previousResult != null && previousResult.CibilScore == report.CibilScore &&
                (previousResult.IsEligible == "Approved" || previousResult.IsEligible == "Rejected"))
            {
                return previousResult;
            }

            string status;
            string? reason = null;
            decimal loanAmount = 0;

            if (report.CibilScore <= 650)
            {
                status = "Rejected";
                reason = "CIBIL score is 650 or less";
            }
            else
            {
                loanAmount = GetMaxEligibleLoanAmount(customer);

                if (loanAmount <= 0)
                {
                    status = "Rejected";
                    reason = "Existing EMI is already more than 40% of monthly income";
                }
                else if (report.CibilScore >= 800)
                {
                    status = "Approved";
                }
                else
                {
                    status = "Manual Review";
                }
            }

            EligibilityResult result = previousResult?.IsEligible == "Manual Review"? previousResult: new EligibilityResult();
            result.CustomerId = customerId;
            result.Customer = customer;
            result.CibilScore = report.CibilScore;
            result.IsEligible = status;
            result.LoanAmount = loanAmount;
            result.RejectionReason = reason;

            if (result.EligibilityId == 0)
            {
                db.EligibilityResults.Add(result);
            }
            await db.SaveChangesAsync();

            return result;
        }

        public decimal GetMaxEligibleLoanAmount(Customer customer)
        {
            decimal best = 0;
            int[] tenures = { 36, 60, 72, 120 };

            foreach (int months in tenures)
            {
                decimal rate = GetTenureRate(months);
                decimal amount = CalculateLoanAmount(customer, months, rate);

                if (amount > best)
                {
                    best = amount;
                }
            }

            return best;
        }

        public async Task<List<ManualReviewDto>> GetManualReviews()
        {
            var customers = await db.Customers.ToListAsync();
            var reviews = new List<ManualReviewDto>();

            foreach (var customer in customers)
            {
                var eligibility = await db.EligibilityResults
                    .Where(e => e.CustomerId == customer.CustomerId)
                    .OrderByDescending(e => e.EligibilityId)
                    .FirstOrDefaultAsync();

                if (eligibility?.IsEligible != "Manual Review")
                {
                    continue;
                }

                var report = await db.CibilReports
                    .Where(r => r.CustomerId == customer.CustomerId)
                    .OrderByDescending(r => r.CheckDate)
                    .FirstOrDefaultAsync();

                if (report == null)
                {
                    continue;
                }

                reviews.Add(new ManualReviewDto
                {
                    EligibilityId = eligibility.EligibilityId,
                    CustomerId = customer.CustomerId,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Age = customer.Age,
                    Email = customer.Email,
                    MobileNo = customer.MobileNo,
                    Pan = customer.Pan,
                    AadhaarNo = customer.AadhaarNo,
                    EmploymentType = customer.EmploymentType,
                    MonthlyIncome = customer.MonthlyIncome,
                    TotalMonthlyDebt = customer.TotalMonthlyDebt,
                    CibilReportId = report.CibilReportId,
                    CibilScore = report.CibilScore,
                    CibilCheckDate = report.CheckDate,
                    CibilStatus = GetCibilStatus(report.CibilScore),
                    LoanAmount = eligibility.LoanAmount,
                    Status = eligibility.IsEligible
                });
            }

            return reviews;
        }

        public async Task<EligibilityResult?> DecideManualReview(ManualReviewDecisionDto decision, int officerId)
        {
            var officer = await db.Users.FirstOrDefaultAsync(u => u.UserId == officerId && u.RoleId == 2);

            if (officer == null || (decision.Decision != "Approved" && decision.Decision != "Rejected"))
            {
                return null;
            }

            var eligibility = await db.EligibilityResults
                .Where(e => e.CustomerId == decision.CustomerId)
                .OrderByDescending(e => e.EligibilityId)
                .FirstOrDefaultAsync();

            if (eligibility == null || eligibility.IsEligible != "Manual Review")
            {
                return null;
            }

            eligibility.IsEligible = decision.Decision;
            eligibility.RejectionReason = string.IsNullOrWhiteSpace(decision.RejectionReason)
                    ? "Rejected by Loan Officer"
                    : decision.RejectionReason; 
            
            await db.SaveChangesAsync();
            return eligibility;
        }

        // Tenure in months (n) and annual rate of interest
        private decimal GetTenureRate(int months)
        {
            if (months == 36)
            {
                return 11.5m;
            }

            if (months == 60 || months == 72 || months == 120)
            {
                return 11m;
            }

            return 0;
        }

        private decimal CalculateLoanAmount(Customer customer, int months, decimal annualRate)
        {
            // Max Allowable EMI = Monthly Income x FOIR %
            decimal maxAllowableEmi = customer.MonthlyIncome * FoirPercent / 100;

            // Available EMI = Max Allowable EMI - Existing Monthly EMI
            decimal availableEmi = maxAllowableEmi - customer.TotalMonthlyDebt;

            if (availableEmi <= 0)
            {
                return 0;
            }

            // Eligible Loan Amount = Available EMI x ((1+r)^n - 1) / (r x (1+r)^n)
            // r = monthly rate of interest, n = tenure in months
            double r = (double)annualRate / 12 / 100;
            double growth = Math.Pow(1 + r, months);
            double amount = (double)availableEmi * (growth - 1) / (r * growth);

            return Math.Round((decimal)amount, 0);
        }

        private string GetCibilStatus(int score)
        {
            if (score >= 900)
            {
                return "Excellent";
            }

            if (score >= 800)
            {
                return "Very Good";
            }

            if (score >= 750)
            {
                return "Good";
            }

            if (score >= 700)
            {
                return "Average";
            }

            return "Risky";
        }
    }
}