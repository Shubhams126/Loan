using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class CibilService : ICibilService
    {
        AppDbContext db;
        public CibilService(AppDbContext db)
        {
            this.db = db;
        }
        public async Task<CibilReport?> GetCibilScore(int customerId)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                return null;
            }

            return await GetCibilScore(customer);
        }


        public async Task<CibilReport> GetCibilScore(Customer customer)
        {
            int incomeScore = 0;
            int employmentScore = 0;
            int ageScore = 0;
            int foirScore = 0;

            // Income score
            if (customer.MonthlyIncome < 25000)
            {
                incomeScore = 100;
            }
            else if (customer.MonthlyIncome < 50000)
            {
                incomeScore = 200;
            }
            else if (customer.MonthlyIncome < 100000)
            {
                incomeScore = 300;
            }
            else
            {
                incomeScore = 400;
            }

            // Employment score
            if (customer.EmploymentType == "Government")
            {
                employmentScore = 200;
            }
            else if (customer.EmploymentType == "Salaried")
            {
                employmentScore = 150;
            }
            else if (customer.EmploymentType == "Self-Employed")
            {
                employmentScore = 100;
            }

            // Age score
            if (customer.Age >= 21 && customer.Age <= 24)
            {
                ageScore = 50;
            }
            else if (customer.Age >= 25 && customer.Age <= 45)
            {
                ageScore = 150;
            }
            else if (customer.Age >= 46 && customer.Age <= 60)
            {
                ageScore = 100;
            }

            // FOIR score
            decimal foir = 0;
            if (customer.MonthlyIncome > 0)
            {
                foir = (customer.TotalMonthlyDebt / customer.MonthlyIncome) * 100;
            }

            if (foir < 30)
            {
                foirScore = 250;
            }
            else if (foir < 50)
            {
                foirScore = 150;
            }
            else if (foir < 60)
            {
                foirScore = 75;
            }
            else
            {
                foirScore = 0;
            }

            // Final CIBIL score
            int totalScore = incomeScore + employmentScore + ageScore + foirScore;

            if (totalScore > 900)
            {
                totalScore = 900;
            }

            CibilReport report = new CibilReport();
            report.CustomerId = customer.CustomerId;
            report.PanNo = customer.Pan;
            report.CibilScore = totalScore;
            report.CheckDate = DateTime.Now;
            report.Customer = customer;

            db.CibilReports.Add(report);
            await db.SaveChangesAsync();

            return report;
        }

        public async Task<CibilReport?> GetLatestCibil(int customerId)
        {
            var report = await db.CibilReports
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CheckDate)
                .FirstOrDefaultAsync();

            return report;
        }
    }
}