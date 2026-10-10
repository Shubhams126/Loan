using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? ChallengeId { get; set; }
        public bool Requires2FASetup { get; set; }
        public string? QRCode { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }

    }
}
