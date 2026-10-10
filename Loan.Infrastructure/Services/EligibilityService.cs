using Loan.Application.Interface;
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

            EligibilityResult result = new EligibilityResult();
            result.CustomerId = customerId;
            result.CibilScore = report.CibilScore;
            result.IsEligible = status;
            result.LoanAmount = loanAmount;
            result.RejectionReason = reason;

            db.EligibilityResults.Add(result);
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
            decimal maxAllowableEmi = customer.MonthlyIncome * FoirPercent;

            // Available EMI = Max Allowable EMI - Existing Monthly EMI
            decimal availableEmi = maxAllowableEmi - 0;

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
    }
}