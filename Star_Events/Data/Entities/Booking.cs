using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Star_Events.Models;

namespace Star_Events.Data.Entities
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        
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

        public DateTime? PaymentDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Navigation properties
        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ApplicationUser Customer { get; set; } = null!;

        // Computed properties
        public bool IsPaid => Status == BookingStatus.Confirmed && PaymentDate.HasValue;
        public bool CanBeCancelled => Status == BookingStatus.Pending;
    }
}


