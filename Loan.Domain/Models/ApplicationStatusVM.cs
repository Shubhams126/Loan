using Microsoft.AspNetCore.Mvc.Rendering;

namespace Loan.Domain.Models
{
    public class ApplicationStatusVM
    {
        public int DealId { get; set; }

        public string LoanType { get; set; }

        public string Profile { get; set; }

        public string Kyc { get; set; }

        public string ApplyLoan { get; set; }

        public string LoanStatus { get; set; }

        public string Sanction { get; set; }

        public string Disbursement { get; set; }

        // NEW FIELDS

        public string LoanAccount { get; set; }

        public string EmiStatus { get; set; }

        public string EmiStatusDetail { get; set; }

        public string Penalty { get; set; }

        public string Foreclosure { get; set; }

        public string LoanClose { get; set; }

        // DROPDOWN

        public List<SelectListItem>? LoanDeals { get; set; }
    }
}