using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Star_Events.Models;

namespace Star_Events.Data.Entities
{
    /// <summary>
    /// Individual ticket entity for each purchased ticket
    /// </summary>
    public class Ticket : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;
        
        [Required]
        public int BookingId { get; set; }
        
        [Required]
        public int BookingItemId { get; set; }
        
        [Required]
        public Guid EventId { get; set; }
        
        [Required]
        public Guid TicketTypeId { get; set; }
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string QRCodeData { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? QRCodeImagePath { get; set; }
        
        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Active;
        
        public DateTime? UsedAt { get; set; }
        
        [StringLength(500)]
        public string? UsedBy { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual BookingItem BookingItem { get; set; } = null!;
        public virtual Event Event { get; set; } = null!;
        public virtual TicketType TicketType { get; set; } = null!;
        public virtual ApplicationUser Customer { get; set; } = null!;
        
        // Computed properties
        public bool IsUsed => Status == TicketStatus.Used;
        public bool IsValid => Status == TicketStatus.Active && Event.Date >= DateTime.Today;
        public string DisplayTicketNumber => $"#{TicketNumber}";
    }
    
    /// <summary>
    /// Enum for ticket status
    /// </summary>
    public enum TicketStatus
    {
        Active = 0,
        Used = 1,
        Cancelled = 2,
        Expired = 3
    }
}
