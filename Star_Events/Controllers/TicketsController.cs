using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Events.Business.Interfaces;
using Star_Events.Data.Entities;
using System.Security.Claims;

namespace Star_Events.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }


        // GET: /Tickets/BookingTickets?bookingId=123
        [HttpGet]
        public async Task<IActionResult> BookingTickets(int bookingId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var tickets = await _ticketService.GetByBookingIdAsync(bookingId);
            
            // Verify that the tickets belong to the current customer
            if (tickets.Any() && tickets.First().CustomerId != customerId)
            {
                return Forbid();
            }

            return View(tickets);
        }

        // GET: /Tickets/Details/123
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null || ticket.CustomerId != customerId)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: /Tickets/QRCode/123
        [HttpGet]
        public async Task<IActionResult> QRCode(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var ticket = await _ticketService.GetByIdAsync(id);

            if (ticket == null || ticket.CustomerId != customerId)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // POST: /Tickets/Validate
        [HttpPost]
        public async Task<IActionResult> Validate(string ticketNumber, Guid eventId)
        {
            var isValid = await _ticketService.ValidateTicketAsync(ticketNumber, eventId);
            
            return Json(new { 
                isValid = isValid,
                message = isValid ? "Ticket is valid" : "Invalid ticket"
            });
        }
    }
}
