using Loan.Domain.Models;
using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IEligibilityService
    {
        Task<EligibilityResult?> CheckEligibility(int customerId, int tenureMonths);
        decimal GetMaxEligibleLoanAmount(Customer customer);
        Task<List<ManualReviewDto>> GetManualReviews();
        Task<EligibilityResult?> DecideManualReview(ManualReviewDecisionDto decision, int officerId);
    }
}
