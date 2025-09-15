using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using Star_Events.Business.Interfaces;
using Star_Events.Models;

namespace Star_Events.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _settings = configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
            _env = env;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, IEnumerable<MimePart>? attachments = null, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            if (attachments != null)
            {
                foreach (var part in attachments)
                {
                    builder.Attachments.Add(part);
                }
            }
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        public async Task SendTemplateAsync(string toEmail, string subject, string templateName, IDictionary<string, string>? placeholders = null, IEnumerable<MimePart>? attachments = null, CancellationToken cancellationToken = default)
        {
            var templatesDir = Path.Combine(_env.WebRootPath, "email-templates");
            var templatePath = Path.Combine(templatesDir, templateName + ".html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templateName}", templatePath);
            }

            var html = await File.ReadAllTextAsync(templatePath, cancellationToken);

            // Remove logo support (no inline images)
            html = html.Replace("{{LogoCid}}", string.Empty);

            if (placeholders != null)
            {
                foreach (var kv in placeholders)
                {
                    html = html.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty);
                }
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();

            // No inline logo attachments

            builder.HtmlBody = html;

            if (attachments != null)
            {
                foreach (var part in attachments)
                {
                    builder.Attachments.Add(part);
                }
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}


