using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class PaymentHistroyDto
    {
        public int LoanPaymentId { get; set; }

        public string LoanAccountNo { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public decimal PaidAmount { get; set; }

        public string PaymentMode { get; set; }

        public string TransactionReference { get; set; }

        public string PaymentStatus { get; set; }

        public int InstallmentNo { get; set; }

        public DateTime DueDate { get; set; }

        public decimal Emi { get; set; }

    }
}
