using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal MonthlyIncome { get; set; }
    }
}
