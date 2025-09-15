using Star_Events.Data.Entities;

namespace Star_Events.Business.Interfaces
{
    public interface ITicketService
    {
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket?> GetByTicketNumberAsync(string ticketNumber);
        Task<IEnumerable<Ticket>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Ticket>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId);
        Task<Ticket> CreateTicketAsync(int bookingId, int bookingItemId, Guid eventId, Guid ticketTypeId, string customerId);
        Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status, string? usedBy = null);
        Task<bool> ValidateTicketAsync(string ticketNumber, Guid eventId);
        Task<IEnumerable<Ticket>> GenerateTicketsForBookingAsync(int bookingId);
    }
}
