using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class ForeClosureRequestDto
    {
        public int RequestId { get; set; }

        public string? RequestNo { get; set; }

        public int LoanAccountId { get; set; }

        public string? LoanAccountNo { get; set; }

        public string? CustomerName { get; set; }

        public string? ForeClosureType { get; set; }

        public DateTime RequestedDate { get; set; }

        public decimal OutstandingPrincipal { get; set; }

        public decimal ForeClosureAmount { get; set; }

        public string? Status { get; set; }

        public string? RequestCategory { get; set; }
    }
}
