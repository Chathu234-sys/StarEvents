namespace Star_Events.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "Star Events";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}


