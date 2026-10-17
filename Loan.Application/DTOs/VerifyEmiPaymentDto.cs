using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class VerifyEmiPaymentDto
    {
        public string RazorpayPaymentId { get; set; }

        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public int EmiScheduleId { get; set; }
        public string PaymentMode { get; set; }
    }
}
