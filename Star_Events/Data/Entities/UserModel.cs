using Star_Events.Helper;

namespace Star_Events.Data.Entities
{
    public class UserModel : CommonProps
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
