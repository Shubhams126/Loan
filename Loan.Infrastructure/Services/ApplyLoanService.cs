
using AutoMapper;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Loan.Infrastructure.Services
{
    public class ApplyLoanService : IApplyLoanService
    {
        private readonly AppDbContext db;
        private readonly IMapper mapper;

        public ApplyLoanService(AppDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public object GetLoanOptions() => new
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

        // Submit loan application
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

            string accountNumber = dto.BankAccountNumber.Trim();
            string ifscCode = dto.IFSCCode.Trim().ToUpperInvariant();

            if (!Regex.IsMatch(accountNumber, @"^[0-9]{9,18}$"))
            {
                dto.Message =
                    "Bank account number must contain 9 to 18 digits.";
                return dto;
            }

            if (!Regex.IsMatch(ifscCode, @"^[A-Z]{4}0[A-Z0-9]{6}$"))
            {
                dto.Message = "Invalid IFSC code.";
                return dto;
            }

            double interestRate = GetInterestRate(dto.TenureMonths);

            if (interestRate == 0)
            {
                dto.Message = "Please select a valid tenure.";
                return dto;
            }

            var loanDeal = mapper.Map<LoanDeal>(dto);

            loanDeal.BankName = dto.BankName.Trim();
            loanDeal.BankAccountNumber = accountNumber;
            loanDeal.IFSCCode = ifscCode;

            loanDeal.InterestRate = interestRate;
            loanDeal.EmiAmount = CalculateEmi(dto.LoanAmount,interestRate, dto.TenureMonths);

            loanDeal.CurrentStatus = "Pending";
            loanDeal.ApprovedAmount = 0;
            loanDeal.RejectionReason = null;
            loanDeal.AppliedDate = DateTime.Now;

            db.LoanDeals.Add(loanDeal);
            await db.SaveChangesAsync();

            var response = mapper.Map<LoanApplyDto>(loanDeal);

            response.Message = "Loan application submitted successfully.";

            return response;
        }

        private static double GetInterestRate(int tenureMonths)
        {
            if (tenureMonths == 36)
                return 11.5;

            if (tenureMonths == 60 ||
                tenureMonths == 72 ||
                tenureMonths == 120)
                return 11.0;

            return 0;
        }

        private static decimal CalculateEmi(
            decimal loanAmount,
            double interestRate,
            int tenureMonths)
        {
            double principal = (double)loanAmount;
            double monthlyRate = interestRate / 12 / 100;

            double factor = Math.Pow(1 + monthlyRate, tenureMonths);

            double emi = principal * monthlyRate * factor / (factor - 1);

            return Math.Round((decimal)emi, 2);
        }
    }
}
