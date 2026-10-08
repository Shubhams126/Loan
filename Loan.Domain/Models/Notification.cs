using System.ComponentModel.DataAnnotations;

namespace Loan.Domain.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int CustomerId { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}