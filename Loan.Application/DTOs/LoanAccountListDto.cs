using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class LoanAccountListDto
    {
        public int LoanAccountId { get; set; }

        public string LoanAccountNo { get; set; }

        public string CustomerName { get; set; }

        public decimal LoanAmount { get; set; }

        public decimal OutstandingPrincipal { get; set; }

        public string Status { get; set; }

        public decimal InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        public DateTime DisbursementDate { get; set; }

        public decimal TotalPaid { get; set; }
    }
}
