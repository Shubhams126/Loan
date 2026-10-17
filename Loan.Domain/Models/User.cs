
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [ForeignKey("RoleId")]
        public int RoleId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsTwoFactorEnabled { get; set; } = false;

        public string? AuthenticatorSecret { get; set; }

        public bool IsAuthenticatorSetupPending { get; set; } = false;

        public string? LoginChallengeId { get; set; }

        public DateTime? LoginChallengeExpiry { get; set; }

        public Role? Role { get; set; }

        public Customer? Customer { get; set; }

        public ICollection<DealReview>? DealReviews { get; set; }
    }
}