using Star_Events.Helper;
using System.ComponentModel;

namespace Star_Events.Data.Entities
{
    public class UserModel : CommonProps
    {
        public int Id { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;
        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
        [DisplayName("Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
