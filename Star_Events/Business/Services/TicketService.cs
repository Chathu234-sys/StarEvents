using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;
using Star_Events.Models;

namespace Star_Events.Business.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly ApplicationDbContext _context;

        public TicketService(
            ITicketRepository ticketRepository, 
            IBookingRepository bookingRepository,
            IQRCodeService qrCodeService,
            ApplicationDbContext context)
        {
            _ticketRepository = ticketRepository;
            _bookingRepository = bookingRepository;
            _qrCodeService = qrCodeService;
            _context = context;
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _ticketRepository.GetByIdAsync(id);
        }

        public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber)
        {
            return await _ticketRepository.GetByTicketNumberAsync(ticketNumber);
        }

        public async Task<IEnumerable<Ticket>> GetByBookingIdAsync(int bookingId)
        {
            return await _ticketRepository.GetByBookingIdAsync(bookingId);
        }

        public async Task<IEnumerable<Ticket>> GetByCustomerIdAsync(string customerId)
        {
            return await _ticketRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId)
        {
            return await _ticketRepository.GetByEventIdAsync(eventId);
        }

        public async Task<Ticket> CreateTicketAsync(int bookingId, int bookingItemId, Guid eventId, Guid ticketTypeId, string customerId)
        {
            var ticketNumber = GenerateTicketNumber();
            var qrData = _qrCodeService.GenerateQRCodeData(ticketNumber, bookingId, eventId);
            var qrImagePath = _qrCodeService.SaveQRCodeImage(qrData, ticketNumber);

            var ticket = new Ticket
            {
                TicketNumber = ticketNumber,
                BookingId = bookingId,
                BookingItemId = bookingItemId,
                EventId = eventId,
                TicketTypeId = ticketTypeId,
                CustomerId = customerId,
                QRCodeData = qrData,
                QRCodeImagePath = qrImagePath,
                Status = TicketStatus.Active,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return await _ticketRepository.AddAsync(ticket);
        }

        public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status, string? usedBy = null)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new InvalidOperationException("Ticket not found");

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (status == TicketStatus.Used)
            {
                ticket.UsedAt = DateTime.UtcNow;
                ticket.UsedBy = usedBy;
            }

            return await _ticketRepository.UpdateAsync(ticket);
        }

        public async Task<bool> ValidateTicketAsync(string ticketNumber, Guid eventId)
        {
            var ticket = await _ticketRepository.GetByTicketNumberAsync(ticketNumber);
            
            if (ticket == null || ticket.EventId != eventId)
                return false;

            if (ticket.Status != TicketStatus.Active)
                return false;

            if (ticket.Event.Date < DateTime.Today)
                return false;

            return true;
        }

        public async Task<IEnumerable<Ticket>> GenerateTicketsForBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException("Booking must be confirmed to generate tickets");

            var tickets = new List<Ticket>();

            foreach (var bookingItem in booking.BookingItems)
            {
                for (int i = 0; i < bookingItem.Quantity; i++)
                {
                    var ticket = await CreateTicketAsync(
                        bookingId, 
                        bookingItem.Id, 
                        booking.EventId, 
                        bookingItem.TicketTypeId, 
                        booking.CustomerId
                    );
                    tickets.Add(ticket);
                }
            }

            return tickets;
        }

        private string GenerateTicketNumber()
        {
            // Generate a unique ticket number
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"TK{timestamp}{random}";
        }
    }
}
