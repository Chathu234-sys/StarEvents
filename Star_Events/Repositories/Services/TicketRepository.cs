using Microsoft.EntityFrameworkCore;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Repositories.Services
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.BookingItem)
                .Include(t => t.Event)
                    .ThenInclude(e => e.Venue)
                .Include(t => t.TicketType)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.BookingItem)
                .Include(t => t.Event)
                    .ThenInclude(e => e.Venue)
                .Include(t => t.TicketType)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber && t.IsActive);
        }

        public async Task<IEnumerable<Ticket>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.BookingItem)
                .Include(t => t.Event)
                    .ThenInclude(e => e.Venue)
                .Include(t => t.TicketType)
                .Include(t => t.Customer)
                .Where(t => t.BookingId == bookingId && t.IsActive)
                .OrderBy(t => t.TicketNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.BookingItem)
                .Include(t => t.Event)
                    .ThenInclude(e => e.Venue)
                .Include(t => t.TicketType)
                .Include(t => t.Customer)
                .Where(t => t.CustomerId == customerId && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.BookingItem)
                .Include(t => t.Event)
                .Include(t => t.TicketType)
                .Include(t => t.Customer)
                .Where(t => t.EventId == eventId && t.IsActive)
                .OrderBy(t => t.TicketNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.BookingItem)
                .Include(t => t.Event)
                .Include(t => t.TicketType)
                .Include(t => t.Customer)
                .Where(t => t.Status == status && t.IsActive)
                .OrderBy(t => t.TicketNumber)
                .ToListAsync();
        }

        public async Task<Ticket> AddAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return false;

            ticket.IsActive = false;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
