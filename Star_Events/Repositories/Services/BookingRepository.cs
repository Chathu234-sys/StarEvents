using Microsoft.EntityFrameworkCore;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Repositories.Services
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(string customerId)
        {
            return await _db.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _db.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Booking booking)
        {
            await _db.Bookings.AddAsync(booking);
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();
            return booking;
        }

        public Task SaveAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}


