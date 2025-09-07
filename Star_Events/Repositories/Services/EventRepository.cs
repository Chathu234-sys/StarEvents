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
            var ev = await GetByIdAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
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
                .Where(s => s.TicketType.EventId == eventId)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        }
    }
}
