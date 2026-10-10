using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loan.Application.DTOs
{
    public class Setup2FADto
    {
        [Required]
        public string ChallengeId { get; set; } = string.Empty;
    }
}
