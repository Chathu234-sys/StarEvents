using MimeKit;
using Star_Events.Repositories.Interfaces;
using Star_Events.Business.Interfaces;

namespace Star_Events.Repositories.Services
{
    // Repository delegates to business service to respect layering
    public class EmailRepository : IEmailRepository
    {
        private readonly IEmailService _emailService;

        public EmailRepository(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task SendAsync(string toEmail, string subject, string htmlBody, IEnumerable<MimePart>? attachments = null, CancellationToken cancellationToken = default)
            => _emailService.SendAsync(toEmail, subject, htmlBody, attachments, cancellationToken);

        public Task SendTemplateAsync(string toEmail, string subject, string templateName, IDictionary<string, string>? placeholders = null, IEnumerable<MimePart>? attachments = null, CancellationToken cancellationToken = default)
            => _emailService.SendTemplateAsync(toEmail, subject, templateName, placeholders, attachments, cancellationToken);
    }
}


