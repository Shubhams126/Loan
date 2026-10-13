using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class ScoreCardService : IScoreCardService
    {
        AppDbContext db;
        IEligibilityService eligibilityService;

        public ScoreCardService(AppDbContext db, IEligibilityService eligibilityService)
        {
            this.db = db;
            this.eligibilityService = eligibilityService;
        }

        public async Task<ScoreCard?> GenerateScoreCard(int customerId)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                return null;
            }

            // Latest CIBIL report of the customer
            var report = await db.CibilReports
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CheckDate)
                .FirstOrDefaultAsync();

            if (report == null)
            {
                return null;
            }

            string riskCategory;
            decimal eligibleAmount = 0;

            if (report.CibilScore <= 650)
            {
                riskCategory = "High";
            }
            else
            {
                if (report.CibilScore < 750)
                {
                    riskCategory = "Medium";
                }
                else
                {
                    riskCategory = "Low";
                }

                eligibleAmount = eligibilityService.GetMaxEligibleLoanAmount(customer);
            }

            // Update existing scorecard or create a new one
            var scoreCard = await db.ScoreCards.FirstOrDefaultAsync(s => s.CustomerId == customerId);
            if (scoreCard == null)
            {
                scoreCard = new ScoreCard();
                scoreCard.CustomerId = customerId;
                db.ScoreCards.Add(scoreCard);
            }

            scoreCard.RiskCategory = riskCategory;
            scoreCard.EligibleLoanAmount = eligibleAmount;

            await db.SaveChangesAsync();
            return scoreCard;
        }

        public async Task<ScoreCard?> GetScoreCard(int customerId)
        {
            return await db.ScoreCards.FirstOrDefaultAsync(s => s.CustomerId == customerId);
        }
    }
}