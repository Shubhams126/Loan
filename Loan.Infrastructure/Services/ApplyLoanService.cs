
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Services
{
    public class ApplyLoanService : IApplyLoanService
    {
        private readonly AppDbContext db;

        public ApplyLoanService(AppDbContext db)
        {
            this.db = db;
        }

        public object GetLoanOptions()
        {
            return new
            {
                LoanTypes = new[]
                {
                    "Home Loan",
                    "Personal Loan",
                    "Educational Loan"
                },
                Tenures = new[]
                {
                    new { Months = 36, Years = 3, InterestRate = 11.5 },
                    new { Months = 60, Years = 5, InterestRate = 11.0 },
                    new { Months = 72, Years = 6, InterestRate = 11.0 },
                    new { Months = 120, Years = 10, InterestRate = 11.0 }
                },
                EmiDates = new[] { 5, 7, 10, 15 }
            };
        }

        public async Task<LoanCalculationDto> CalculateLoanAsync(
            int customerId,
            decimal loanAmount,
            int tenureYears)
        {
            var result = new LoanCalculationDto();

            var customer = await db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);

            if (customer == null)
            {
                result.Message = "Customer not found.";
                return result;
            }

            if (loanAmount <= 0)
            {
                result.Message = "Loan amount must be greater than zero.";
                return result;
            }

            decimal eligibleAmount = customer.MonthlyIncome * 20;

            if (loanAmount > eligibleAmount)
            {
                result.Message =
                    $"Maximum eligible amount is ₹{eligibleAmount:N0}.";
                return result;
            }

            int tenureMonths = tenureYears * 12;
            double interestRate = GetInterestRate(tenureMonths);

            if (interestRate == 0)
            {
                result.Message = "Please select a valid tenure.";
                return result;
            }

            result.InterestRate = interestRate;
            result.EmiAmount = CalculateEmi(
                loanAmount,
                interestRate,
                tenureMonths);

            result.Message = "Loan calculation completed successfully.";

            return result;
        }

        public async Task<LoanApplyDto> ApplyLoanAsync(LoanApplyDto dto)
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(x => x.CustomerId == dto.CustomerId);

            if (customer == null)
            {
                dto.Message = "Customer not found.";
                return dto;
            }

            if (dto.LoanAmount <= 0)
            {
                dto.Message = "Loan amount must be greater than zero.";
                return dto;
            }

            decimal eligibleAmount = customer.MonthlyIncome * 20;

            if (dto.LoanAmount > eligibleAmount)
            {
                dto.Message =
                    $"Maximum eligible amount is ₹{eligibleAmount:N0}.";
                return dto;
            }

            if (dto.LoanType != "Home Loan" &&
                dto.LoanType != "Personal Loan" &&
                dto.LoanType != "Educational Loan")
            {
                dto.Message = "Please select a valid loan type.";
                return dto;
            }

            if (dto.TenureMonths != 36 &&
                dto.TenureMonths != 60 &&
                dto.TenureMonths != 72 &&
                dto.TenureMonths != 120)
            {
                dto.Message = "Please select a valid tenure.";
                return dto;
            }

            if (dto.EmiDay != 5 &&
                dto.EmiDay != 7 &&
                dto.EmiDay != 10 &&
                dto.EmiDay != 15)
            {
                dto.Message = "Please select a valid EMI date.";
                return dto;
            }

            if (string.IsNullOrWhiteSpace(dto.BankName) ||
                string.IsNullOrWhiteSpace(dto.BankAccountNumber) ||
                string.IsNullOrWhiteSpace(dto.IFSCCode))
            {
                dto.Message = "Please enter valid bank details.";
                return dto;
            }

            double interestRate = GetInterestRate(dto.TenureMonths);

            decimal emiAmount = CalculateEmi(
                dto.LoanAmount,
                interestRate,
                dto.TenureMonths);

            var loanDeal = new LoanDeal
            {
                CustomerId = dto.CustomerId,
                LoanType = dto.LoanType,
                LoanAmount = dto.LoanAmount,
                InterestRate = interestRate,
                TenureMonths = dto.TenureMonths,
                EmiAmount = emiAmount,
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                IFSCCode = dto.IFSCCode,
                EmiDay = dto.EmiDay,
                ApprovedAmount = 0,
                CurrentStatus = "Pending",
                RejectionReason = null,
                AppliedDate = DateTime.Now
            };

            db.LoanDeals.Add(loanDeal);
            await db.SaveChangesAsync();

            dto.DealId = loanDeal.DealId;
            dto.InterestRate = loanDeal.InterestRate;
            dto.EmiAmount = loanDeal.EmiAmount;
            dto.CurrentStatus = loanDeal.CurrentStatus;
            dto.AppliedDate = loanDeal.AppliedDate;
            dto.Message = "Loan application submitted successfully.";

            return dto;
        }

        public async Task<List<LoanApplyDto>> GetPendingLoansAsync()
        {
            var loans = await db.LoanDeals
                .AsNoTracking()
                .Where(x => x.CurrentStatus == "Pending")
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            return loans.Select(MapToDto).ToList();
        }

        public async Task<LoanApplyDto?> GetLoanByIdAsync(int dealId)
        {
            var loanDeal = await db.LoanDeals
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loanDeal == null)
            {
                return null;
            }

            return MapToDto(loanDeal);
        }

        public async Task<bool> ApproveLoanAsync(
            int dealId,
            LoanDealReviewDto dto,
            int officerId)
        {
            var loanDeal = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loanDeal == null ||
                loanDeal.CurrentStatus != "Pending")
            {
                return false;
            }

            if (dto.ApprovedAmount <= 0 ||
                dto.ApprovedAmount > loanDeal.LoanAmount)
            {
                return false;
            }

            loanDeal.ApprovedAmount = dto.ApprovedAmount;
            loanDeal.CurrentStatus = "Approved";
            loanDeal.RejectionReason = null;

            loanDeal.EmiAmount = CalculateEmi(
                dto.ApprovedAmount,
                loanDeal.InterestRate,
                loanDeal.TenureMonths);

            db.DealReviews.Add(new DealReview
            {
                DealId = loanDeal.DealId,
                OfficerId = officerId,
                Status = "Approved",
                Remarks = "Loan approved.",
                ReviewedAt = DateTime.Now
            });

            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectLoanAsync(
            int dealId,
            LoanDealReviewDto dto,
            int officerId)
        {
            var loanDeal = await db.LoanDeals
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loanDeal == null ||
                loanDeal.CurrentStatus != "Pending")
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                return false;
            }

            loanDeal.CurrentStatus = "Rejected";
            loanDeal.ApprovedAmount = 0;
            loanDeal.RejectionReason = dto.RejectionReason.Trim();

            db.DealReviews.Add(new DealReview
            {
                DealId = loanDeal.DealId,
                OfficerId = officerId,
                Status = "Rejected",
                Remarks = dto.RejectionReason.Trim(),
                ReviewedAt = DateTime.Now
            });

            await db.SaveChangesAsync();

            return true;
        }

        private double GetInterestRate(int tenureMonths)
        {
            if (tenureMonths == 36)
            {
                return 11.5;
            }

            if (tenureMonths == 60 ||
                tenureMonths == 72 ||
                tenureMonths == 120)
            {
                return 11.0;
            }

            return 0;
        }

        private decimal CalculateEmi(
            decimal loanAmount,
            double interestRate,
            int tenureMonths)
        {
            double principal = (double)loanAmount;
            double monthlyRate = interestRate / 12 / 100;
            double factor = Math.Pow(1 + monthlyRate, tenureMonths);

            double emi =
                principal * monthlyRate * factor / (factor - 1);

            return Math.Round((decimal)emi, 2);
        }

        private LoanApplyDto MapToDto(LoanDeal loanDeal)
        {
            return new LoanApplyDto
            {
                DealId = loanDeal.DealId,
                CustomerId = loanDeal.CustomerId,
                LoanType = loanDeal.LoanType,
                LoanAmount = loanDeal.LoanAmount,
                InterestRate = loanDeal.InterestRate,
                TenureMonths = loanDeal.TenureMonths,
                EmiAmount = loanDeal.EmiAmount,
                BankName = loanDeal.BankName,
                BankAccountNumber = loanDeal.BankAccountNumber,
                IFSCCode = loanDeal.IFSCCode,
                EmiDay = loanDeal.EmiDay,
                CurrentStatus = loanDeal.CurrentStatus,
                RejectionReason = loanDeal.RejectionReason,
                AppliedDate = loanDeal.AppliedDate,
                Message = "Loan details retrieved successfully."
            };
        }
    }
}
