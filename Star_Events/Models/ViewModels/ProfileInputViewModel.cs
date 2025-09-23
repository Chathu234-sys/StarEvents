using System.ComponentModel.DataAnnotations;

namespace Star_Events.Models.ViewModels
{
    public class ProfileInputViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "First Name")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(30)]
        public string? ContactNumber { get; set; }
    }
}






