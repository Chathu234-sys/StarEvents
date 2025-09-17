using Microsoft.EntityFrameworkCore;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Repositories.Services
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.TicketTypes)
                .ToListAsync();
        }

        public async Task AddAsync(Event ev)
        {
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event ev)
        {
            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            // Delete in dependency-safe order to satisfy FK constraints
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
                return;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Remove Tickets for this Event (they reference TicketType and BookingItem with Restrict)
                var tickets = await _context.Tickets
                    .Where(t => t.EventId == id)
                    .ToListAsync();
                if (tickets.Count > 0)
                {
                    _context.Tickets.RemoveRange(tickets);
                    await _context.SaveChangesAsync();
                }

                // 2) Remove Bookings for this Event (Payments and BookingItems are set to Cascade on Booking)
                var bookings = await _context.Bookings
                    .Where(b => b.EventId == id)
                    .ToListAsync();
                if (bookings.Count > 0)
                {
                    _context.Bookings.RemoveRange(bookings);
                    await _context.SaveChangesAsync();
                }

                // 3) Remove TicketSales related to this Event or its TicketTypes
                var ticketTypeIds = await _context.TicketTypes
                    .Where(tt => tt.EventId == id)
                    .Select(tt => tt.Id)
                    .ToListAsync();

                if (ticketTypeIds.Count > 0)
                {
                    var ticketSales = await _context.TicketSales
                        .Where(ts => ts.EventId == id || ticketTypeIds.Contains(ts.TicketTypeId))
                        .ToListAsync();
                    if (ticketSales.Count > 0)
                    {
                        _context.TicketSales.RemoveRange(ticketSales);
                        await _context.SaveChangesAsync();
                    }
                }

                // 4) Remove TicketTypes for this Event (now safe after removing Tickets and BookingItems via Booking cascade)
                var ticketTypes = await _context.TicketTypes
                    .Where(tt => tt.EventId == id)
                    .ToListAsync();
                if (ticketTypes.Count > 0)
                {
                    _context.TicketTypes.RemoveRange(ticketTypes);
                    await _context.SaveChangesAsync();
                }

                // 5) Finally remove the Event
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId)
        {
            return await _context.TicketTypes.Where(t => t.EventId == eventId).ToListAsync();
        }

        public async Task AddTicketTypeAsync(TicketType type)
        {
            _context.TicketTypes.Add(type);
            await _context.SaveChangesAsync();
        }

        public async Task RecordSaleAsync(TicketSale sale)
        {
            _context.TicketSales.Add(sale);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid eventId)
        {
            return await _context.TicketSales
                .Where(s => s.EventId == eventId)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        }
    }
}
