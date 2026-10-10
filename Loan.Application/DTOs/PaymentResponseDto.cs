using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
     public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int LoanAccountId { get; set; }
        public int EmiScheduleId { get; set; }
        public int InstallmentNo { get; set; }
        public double EmiAmount { get; set; }
        public double PrincipalPaid { get; set; }
        public double InterestPaid { get; set; }
        public double PenaltyPaid { get; set; }
        public double TotalPaidAmount { get; set; }
        public string PaymentMode { get; set; }
        public string TransactionReference { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

    }
}
