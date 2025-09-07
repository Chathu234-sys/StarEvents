using System.ComponentModel.DataAnnotations;

namespace Star_Events.Data.Entities
{
    public class TicketSale
    {
        [Key]
        public Guid Id { get; set; }

        public Guid TicketTypeId { get; set; }
        public TicketType TicketType { get; set; } = null!;

        public int Quantity { get; set; }

        [Range(0, 999999)]
        public decimal TotalAmount { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    }
}
