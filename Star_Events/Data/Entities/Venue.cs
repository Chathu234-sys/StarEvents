using System.ComponentModel.DataAnnotations;

namespace Star_Events.Data.Entities
{
    public class Venue
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableSeats { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}


