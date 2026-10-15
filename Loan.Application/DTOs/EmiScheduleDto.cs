using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class EmiScheduleDto
    {
        public int EmiScheduleId { get; set; }

        public int LoanAccountId { get; set; }

        public int InstallmentNo { get; set; }

        public DateTime DueDate { get; set; }

        public decimal Emi { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal ClosingBalance { get; set; }

        public string PaymentStatus { get; set; }

        public DateTime? PaidDate { get; set; }
    }
}
