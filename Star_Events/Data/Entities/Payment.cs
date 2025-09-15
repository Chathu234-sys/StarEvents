using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Star_Events.Models;

namespace Star_Events.Data.Entities
{
    /// <summary>
    /// Payment model for handling payment transactions
    /// Demonstrates inheritance from BaseEntity and encapsulation
    /// </summary>
    public class Payment : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string PaymentId { get; set; } = string.Empty;
        
        [Required]
        public int BookingId { get; set; }
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public PaymentMethod Method { get; set; }
        
        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
        [StringLength(500)]
        public string? TransactionId { get; set; }
        
        [StringLength(1000)]
        public string? PaymentDetails { get; set; }
        
        public DateTime? ProcessedAt { get; set; }
        
        [StringLength(500)]
        public string? FailureReason { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual ApplicationUser Customer { get; set; } = null!;
        
        // Computed properties demonstrating encapsulation
        public bool IsSuccessful => Status == PaymentStatus.Completed;
        public bool IsFailed => Status == PaymentStatus.Failed;
        public bool IsPending => Status == PaymentStatus.Pending;
    }
    
    /// <summary>
    /// Enum for payment methods
    /// </summary>
    public enum PaymentMethod
    {
        Stripe
    }
    
    /// <summary>
    /// Enum for payment status
    /// </summary>
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}
