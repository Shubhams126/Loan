using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class CreateEmiPaymentDto
    {
        public int LoanAccountId { get; set; }
        public int EmiScheduleId { get; set; }

        public string PaymentMode { get; set; }
    }
}
