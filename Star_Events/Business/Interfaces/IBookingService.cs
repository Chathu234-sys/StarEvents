using Star_Events.Data.Entities;

namespace Star_Events.Business.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetCustomerBookingsAsync(string customerId);
        Task<Booking?> GetAsync(int id);
        Task<Booking> CreateAsync(string customerId, Guid eventId, IDictionary<Guid, int> ticketTypeToQuantity);
        Task<bool> ConfirmPaymentAsync(int bookingId);
    }
}


