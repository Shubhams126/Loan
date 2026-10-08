using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class CibilReport
    {
        [Key]
        public int CibilReportId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public string PanNo { get; set; }

        public int CibilScore { get; set; }

        public DateTime CheckDate { get; set; }

        public Customer? Customer { get; set; }
    }
}