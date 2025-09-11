using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Star_Events.Data.Entities
{
    public partial class Event
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;


        [DisplayName("Banner")]
        public string? PosterUrl { get; set; }

        public Guid? VenueId { get; set; }
        public Venue? Venue { get; set; }

        [Required]
        public string ManagerId { get; set; } = string.Empty;
        public UserModel? Users { get; set; } 

        public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
        public ICollection<TicketSale> TicketSales { get; set; } = new List<TicketSale>();
    }
}

