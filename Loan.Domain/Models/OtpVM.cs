using System.ComponentModel.DataAnnotations;

namespace Loan.Domain.ViewModels
{
    public class OtpVM
    {
        public string Email { get; set; }

        [Required]
        public string OTP { get; set; }
    }
}