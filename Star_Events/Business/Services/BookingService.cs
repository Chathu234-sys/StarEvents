using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Business.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBookingRepository _repo;

        public BookingService(ApplicationDbContext db, IBookingRepository repo)
        {
            _db = db;
            _repo = repo;
        }

        public Task<IEnumerable<Booking>> GetCustomerBookingsAsync(string customerId)
        {
            return _repo.GetByCustomerIdAsync(customerId);
        }

        public Task<Booking?> GetAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<Booking> CreateAsync(string customerId, Guid eventId, IDictionary<Guid, int> ticketTypeToQuantity)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var evt = await _db.Events
                .Include(e => e.TicketTypes)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (evt == null)
            {
                throw new InvalidOperationException("Event not found");
            }

            var requestedIds = ticketTypeToQuantity.Keys.ToList();
            var ticketTypes = await _db.TicketTypes
                .Where(t => requestedIds.Contains(t.Id) && t.EventId == eventId)
                .ToDictionaryAsync(t => t.Id);

            var items = new List<BookingItem>();
            decimal total = 0m;

            foreach (var (ticketTypeId, rawQty) in ticketTypeToQuantity)
            {
                if (!ticketTypes.TryGetValue(ticketTypeId, out var ticketType))
                {
                    continue; // skip unknown
                }

                var qty = Math.Max(0, rawQty);
                if (qty == 0) continue;

                if (qty > ticketType.TotalAvailable)
                {
                    throw new InvalidOperationException($"Only {ticketType.TotalAvailable} tickets left for {ticketType.Name}.");
                }

                var unit = ticketType.Price;
                var line = unit * qty;
                total += line;

                items.Add(new BookingItem
                {
                    TicketTypeId = ticketType.Id,
                    Quantity = qty,
                    UnitPrice = unit,
                    TotalPrice = line
                });
            }

            if (items.Count == 0)
            {
                throw new InvalidOperationException("No tickets selected");
            }

            var booking = new Booking
            {
                BookingNumber = await GenerateNumberAsync(),
                CustomerId = customerId,
                EventId = eventId,
                BookingDate = DateTime.UtcNow,
                TotalAmount = total,
                DiscountAmount = 0,
                FinalAmount = total,
                Status = BookingStatus.Pending,
                BookingItems = items
            };

            await _repo.AddAsync(booking);
            await _repo.SaveAsync();

            // Decrement availability
            foreach (var item in items)
            {
                var tt = ticketTypes[item.TicketTypeId];
                tt.TotalAvailable -= item.Quantity;
                if (tt.TotalAvailable < 0) tt.TotalAvailable = 0;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return booking;
        }

        private async Task<string> GenerateNumberAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var rnd = Random.Shared.Next(1000, 9999);
            var number = $"BK{timestamp}{rnd}";
            var exists = await _db.Bookings.AnyAsync(b => b.BookingNumber == number);
            if (!exists) return number;
            return await GenerateNumberAsync();
        }
    }
}


