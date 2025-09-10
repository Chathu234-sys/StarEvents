using System.ComponentModel.DataAnnotations;

namespace Star_Events.Data.Entities
{
    public class TicketType
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // e.g., VIP, Regular, Student

        public string Description { get; set; } = string.Empty;

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public int TotalAvailable { get; set; }

        // Relationship
        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;
    }
}
