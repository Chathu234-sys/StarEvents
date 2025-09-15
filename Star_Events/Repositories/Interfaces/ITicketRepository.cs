using Star_Events.Data.Entities;

namespace Star_Events.Repositories.Interfaces
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket?> GetByTicketNumberAsync(string ticketNumber);
        Task<IEnumerable<Ticket>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Ticket>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId);
        Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status);
        Task<Ticket> AddAsync(Ticket ticket);
        Task<Ticket> UpdateAsync(Ticket ticket);
        Task<bool> DeleteAsync(int id);
    }
}
