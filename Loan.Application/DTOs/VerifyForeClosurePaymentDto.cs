using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class VerifyForeClosurePaymentDto
    {
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }

        public int RequestId { get; set; }
        public string PaymentMode { get; set; }
    }
}
