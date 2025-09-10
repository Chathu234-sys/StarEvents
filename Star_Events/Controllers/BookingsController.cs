using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data;
using Star_Events.Data.Entities;
using System.Security.Claims;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _service;
        private readonly ApplicationDbContext _db;

        public BookingsController(IBookingService service, ApplicationDbContext db)
        {
            _service = service;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var bookings = await _service.GetCustomerBookingsAsync(customerId);
            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var booking = await _service.GetAsync(id);
            if (booking == null || booking.CustomerId != customerId) return NotFound();
            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid eventId)
        {
            var evt = await _db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == eventId);
            if (evt == null) return NotFound();
            ViewBag.Event = evt;
            ViewBag.TicketTypes = evt.TicketTypes;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid eventId, Dictionary<Guid, int> ticketQuantities)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(customerId)) return Challenge();

            var clean = ticketQuantities.Where(kv => kv.Value > 0).ToDictionary(k => k.Key, v => v.Value);
            if (clean.Count == 0)
            {
                TempData["Error"] = "Please select at least one ticket.";
                return RedirectToAction(nameof(Create), new { eventId });
            }

            var booking = await _service.CreateAsync(customerId, eventId, clean);
            TempData["SuccessMessage"] = "Booking created successfully";
            return RedirectToAction(nameof(Details), new { id = booking.Id });
        }
    }
}


