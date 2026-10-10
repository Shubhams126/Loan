using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loan.Application.DTOs
{
    public class VerifyOTPDto
    {
        [Required]
        public string ChallengeId { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$")]
        public string OTP { get; set; } = string.Empty;
    }
}
