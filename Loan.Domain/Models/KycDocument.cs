using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loan.Domain.Models
{
    public class KycDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public string DocumentType { get; set; }

        public string FilePath { get; set; }

        public string VerificationStatus { get; set; }

        public Customer? Customer { get; set; }
    }
}