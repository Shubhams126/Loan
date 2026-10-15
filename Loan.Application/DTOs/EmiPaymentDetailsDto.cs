using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class EmiPaymentDetailsDto
    {
        public int EmiScheduleId { get; set; }
        public int LoanAccountId { get; set; }
        public string LoanAccountNo { get; set; }

        public DateTime DueDate { get; set; }

        public decimal EmiAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal LateFees { get; set; }
        public decimal TotalPayableAmount { get; set; }

        public string PaymentStatus { get; set; }
    }
}
