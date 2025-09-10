using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Star_Events.Data.Entities
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Expired = 3
    }

    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string? PromoCode { get; set; }
        public int? LoyaltyPointsUsed { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}


