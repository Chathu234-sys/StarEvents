using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Star_Events.Models.ViewModels
{
    public class AdminProfileViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required (ErrorMessage ="Can not be empty!")]
        [DisplayName("User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Can not be empty!")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Can not be empty!")]
        [DisplayName("First Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Can not be empty!")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Can not be empty!")]
        [DisplayName("Contact Number")]
        [MaxLength(10, ErrorMessage = "Contact number cannot exceed 10 digits")]
        public string ContactNumber { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}
