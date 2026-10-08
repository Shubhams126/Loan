using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class PenaltyCharge
    {
        [Key]
        public int PenaltyId { get; set; }

        [ForeignKey("LoanAccount")]
        public int LoanAccountId { get; set; }

        [ForeignKey("EmiSchedule")]
        public int EmiScheduleId { get; set; }

        public string PenaltyType { get; set; }

        public decimal PenaltyAmount { get; set; }

        public int DelayDays { get; set; }

        public DateTime AppliedDate { get; set; }

        public string Status { get; set; }

        public LoanAccount? LoanAccount { get; set; }

        public EmiSchedule? EmiSchedule { get; set; }
    }
}