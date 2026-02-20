using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.PaymentDto
{
    public class ProcessWithdrawalDto
    {
        [Required(ErrorMessage = "Status is required (Approved/Rejected)")]
        public bool IsApproved { get; set; }

        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        [MaxLength(500, ErrorMessage = "Admin notes cannot exceed 500 characters")]
        public string? AdminNotes { get; set; }

        [MaxLength(255, ErrorMessage = "Transaction ID cannot exceed 255 characters")]
        public string? TransactionId { get; set; }
    }
}
