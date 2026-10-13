using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class CreateForeclosureDto
    {
        public int RequestId { get; set; }
        public int LoanAccountId { get; set; }
        public string ForeClosureType { get; set; }

        public decimal ForeClosureAmount { get; set; }
        public decimal PartialAmount { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime ExpectedClosureDate { get; set; }
        public string Reason { get; set; }

        public DateTime? ApprovedDate { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidDate { get; set; }
    }
}
