using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Star_Events.Models.ViewModels
{
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(100, ErrorMessage = "Event name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [Required(ErrorMessage = "Event time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; } = new TimeSpan(18, 0, 0);

        [Required(ErrorMessage = "Event category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid? VenueId { get; set; }

        public IEnumerable<SelectListItem> Venues { get; set; } = Enumerable.Empty<SelectListItem>();

        // Ticket types
        [Range(0, double.MaxValue, ErrorMessage = "VIP price must be 0 or greater.")]
        public decimal VipPrice { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "VIP total must be 0 or greater.")]
        public int VipTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Seating price must be 0 or greater.")]
        public decimal SeatingPrice { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Seating total must be 0 or greater.")]
        public int SeatingTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Standing price must be 0 or greater.")]
        public decimal StandingPrice { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Standing total must be 0 or greater.")]
        public int StandingTotal { get; set; }
    }
}


