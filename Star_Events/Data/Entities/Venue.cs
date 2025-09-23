using System.ComponentModel.DataAnnotations;

namespace Star_Events.Data.Entities
{
    public class Venue
    {
       
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Venue name is required.")]
        [StringLength(150, ErrorMessage = "Venue name cannot exceed 150 characters.")]
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


