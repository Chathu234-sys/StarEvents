using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Star_Events.Business.Interfaces;

namespace Star_Events.Business.Services
{

    public class QRCodeService : IQRCodeService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public QRCodeService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public string GenerateQRCodeData(string ticketNumber, int bookingId, Guid eventId)
        {
            // Create a unique QR code data that includes ticket validation information
            var qrData = new
            {
                ticketNumber = ticketNumber,
                bookingId = bookingId,
                eventId = eventId,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                type = "event_ticket"
            };

            return System.Text.Json.JsonSerializer.Serialize(qrData);
        }

        public byte[] GenerateQRCodeImage(string qrData, int size = 200)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                using (var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q))
                {
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        var qrCodeBytes = qrCode.GetGraphic(size);
                        return qrCodeBytes;
                    }
                }
            }
        }

        public string SaveQRCodeImage(string qrData, string ticketNumber, int size = 200)
        {
            try
            {
                // Create qr-codes directory if it doesn't exist
                var qrCodesPath = Path.Combine(_webHostEnvironment.WebRootPath, "qr-codes");
                if (!Directory.Exists(qrCodesPath))
                {
                    Directory.CreateDirectory(qrCodesPath);
                }

                // Generate QR code image
                var qrCodeBytes = GenerateQRCodeImage(qrData, size);
                
                // Save to file
                var fileName = $"ticket_{ticketNumber}_{DateTime.UtcNow:yyyyMMdd}.png";
                var filePath = Path.Combine(qrCodesPath, fileName);
                
                File.WriteAllBytes(filePath, qrCodeBytes);
                
                // Return relative path for web access
                return $"/qr-codes/{fileName}";
            }
            catch (Exception ex)
            {
                // Log error and return empty string
                Console.WriteLine($"Error saving QR code: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
