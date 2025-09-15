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
        private readonly IEmailService _emailService;

        public BookingsController(IBookingService service, ApplicationDbContext db, IEmailService emailService)
        {
            _service = service;
            _db = db;
            _emailService = emailService;
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

            // Send booking confirmation email (template)
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
                if (!string.IsNullOrWhiteSpace(userEmail) && evt != null)
                {
                    var subject = $"Your booking is created - {evt.Name}";
                    var placeholders = new Dictionary<string,string>
                    {
                        {"FirstName", User.Identity?.Name ?? "Customer"},
                        {"BookingNo", booking.Id.ToString("D6")},
                        {"EventName", evt.Name},
                        {"EventDate", evt.Date.ToString("MMM dd, yyyy")},
                        {"TotalAmount", booking.FinalAmount.ToString("N0")},
                        {"BrandName", "Star Events"}
                    };
                    await _emailService.SendTemplateAsync(userEmail, subject, "booking-created", placeholders);
                }
            }
            catch { }

            TempData["SuccessMessage"] = "Booking created successfully";
            return RedirectToAction(nameof(Details), new { id = booking.Id });
        }
    }
}


