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
                FinalAmount = total,
                Status = BookingStatus.Pending,
                BookingItems = items
            };

            await _repo.AddAsync(booking);
            await _repo.SaveAsync();

            // Decrement availability and record ticket sales
            var ticketSales = new List<TicketSale>();
            foreach (var item in items)
            {
                var tt = ticketTypes[item.TicketTypeId];
                tt.TotalAvailable -= item.Quantity;
                if (tt.TotalAvailable < 0) tt.TotalAvailable = 0;

                // Record ticket sale
                ticketSales.Add(new TicketSale
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    TicketTypeId = item.TicketTypeId,
                    Quantity = item.Quantity,
                    TotalAmount = item.TotalPrice,
                    CustomerId = customerId,
                    Status = "Pending", // Will be updated to "Confirmed" after payment
                    SaleDate = DateTime.UtcNow
                });
            }

            // Add ticket sales to database
            _db.TicketSales.AddRange(ticketSales);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return booking;
        }

        public async Task<bool> ConfirmPaymentAsync(int bookingId)
        {
            var booking = await _db.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null || booking.Status != BookingStatus.Pending)
                return false;

            using var tx = await _db.Database.BeginTransactionAsync();

            // Update booking status
            booking.Status = BookingStatus.Confirmed;
            booking.PaymentDate = DateTime.UtcNow;

            // Update related ticket sales status
            var ticketSales = await _db.TicketSales
                .Where(ts => ts.EventId == booking.EventId && ts.CustomerId == booking.CustomerId)
                .ToListAsync();

            foreach (var sale in ticketSales)
            {
                sale.Status = "Confirmed";
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
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


