using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Star_Events.Models.ViewModels
{
    public class EventCreateViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; } = new TimeSpan(18, 0, 0);

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public Guid? VenueId { get; set; }

        public IEnumerable<SelectListItem> Venues { get; set; } = Enumerable.Empty<SelectListItem>();

        // Ticket types
        public decimal VipPrice { get; set; }
        public int VipTotal { get; set; }

        public decimal RegularPrice { get; set; }
        public int RegularTotal { get; set; }

        public decimal ChildrenPrice { get; set; }
        public int ChildrenTotal { get; set; }
    }
}


