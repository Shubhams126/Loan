using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class CreateOrderDto
    {
        public int EmiScheduleId { get; set; }

        public decimal Amount { get; set; }
    }
}
