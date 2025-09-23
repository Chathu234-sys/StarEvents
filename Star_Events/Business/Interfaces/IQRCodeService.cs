namespace Star_Events.Business.Interfaces
{
    public interface IQRCodeService
    {
        string GenerateQRCodeData(string ticketNumber, int bookingId, Guid eventId);
        byte[] GenerateQRCodeImage(string qrData, int size = 200);
        string SaveQRCodeImage(string qrData, string ticketNumber, int size = 200);
    }
}


