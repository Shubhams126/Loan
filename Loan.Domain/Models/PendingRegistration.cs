using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loan.Domain.Models
{
    public class PendingRegistration
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string MobileNo { get; set; } = "";
        public string Pan { get; set; } = "";
        public string AadhaarNo { get; set; } = "";
        public string EmploymentType { get; set; } = "";
        public decimal MonthlyIncome { get; set; }
        public decimal TotalMonthlyDebt { get; set; }

        public string OtpHash { get; set; } = "";
        public DateTime OtpExpiry { get; set; }
    }
}
