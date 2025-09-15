using MimeKit;

namespace Star_Events.Repositories.Interfaces
{
    public interface IEmailRepository
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, IEnumerable<MimePart>? attachments = null, CancellationToken cancellationToken = default);

        Task SendTemplateAsync(
            string toEmail,
            string subject,
            string templateName,
            IDictionary<string, string>? placeholders = null,
            IEnumerable<MimePart>? attachments = null,
            CancellationToken cancellationToken = default);
    }
}


