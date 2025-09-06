using System.ComponentModel.DataAnnotations;

namespace Star_Events.Data.Entities
{
    public class Event
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 999999)]
        public decimal TicketPrice { get; set; }

        public string? PosterUrl { get; set; }

        public Guid? VenueId { get; set; }
        public Venue? Venue { get; set; }

        // NEW: Ticket types
        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();

        // NEW: Sales records
        public ICollection<TicketSale> TicketSales { get; set; } = new List<TicketSale>();
    }
}
