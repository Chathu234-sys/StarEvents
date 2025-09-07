using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Star_Events.Data.Entities
{
    public class BookingItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        [Required]
        public Guid TicketTypeId { get; set; }
        public TicketType TicketType { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}


