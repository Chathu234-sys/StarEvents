using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Star_Events.Business.Interfaces;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Models;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace Star_Events.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly ITicketService _ticketService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly StripeSettings _stripeSettings;

        public PaymentsController(
            IPaymentService paymentService, 
            IBookingService bookingService,
            ITicketService ticketService,
            ApplicationDbContext context,
            IEmailService emailService,
            IOptions<StripeSettings> stripeOptions)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _ticketService = ticketService;
            _context = context;
            _emailService = emailService;
            _stripeSettings = stripeOptions.Value;
            
            // Initialize Stripe
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        // GET: /Payments/Checkout?bookingId=123
        [HttpGet]
        public async Task<IActionResult> Checkout(int bookingId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == customerId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index", "Bookings");
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["ErrorMessage"] = "This booking is not eligible for payment.";
                return RedirectToAction("Details", "Bookings", new { id = bookingId });
            }

            try
            {
                // Create Stripe checkout session
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(booking.FinalAmount * 100), // Convert to cents
                                Currency = "lkr",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Event Tickets - {booking.Event.Name}",
                                    Description = $"Booking #{booking.Id} - {booking.BookingItems.Count} ticket(s)"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = Url.Action("Success", "Payments", new { bookingId = bookingId }, Request.Scheme),
                    CancelUrl = Url.Action("Cancel", "Payments", new { bookingId = bookingId }, Request.Scheme),
                    CustomerEmail = User.FindFirstValue(ClaimTypes.Email),
                    Metadata = new Dictionary<string, string>
                    {
                        { "bookingId", bookingId.ToString() },
                        { "customerId", customerId }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // Create payment record
                var payment = await _paymentService.CreatePaymentAsync(
                    bookingId, 
                    customerId, 
                    booking.FinalAmount, 
                    Star_Events.Data.Entities.PaymentMethod.Stripe, 
                    session.Id);

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating payment session: {ex.Message}";
                return RedirectToAction("Details", "Bookings", new { id = bookingId });
            }
        }


        // GET: /Payments/Success?bookingId=123
        [HttpGet]
        public async Task<IActionResult> Success(int bookingId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == customerId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index", "Bookings");
            }

            // Update booking status to confirmed
            booking.Status = BookingStatus.Confirmed;
            booking.PaymentDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update payment status
            var payments = await _paymentService.GetByBookingIdAsync(bookingId);
            foreach (var payment in payments.Where(p => p.Status == PaymentStatus.Pending))
            {
                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTime.UtcNow;
                await _paymentService.UpdateAsync(payment);
            }

            // Generate tickets for the booking
            try
            {
                var tickets = await _ticketService.GenerateTicketsForBookingAsync(bookingId);
                TempData["SuccessMessage"] = $"Payment completed successfully! {tickets.Count()} tickets have been generated for your booking.";

                // Email tickets with QR attachments (template)
                try
                {
                    var customerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(customerEmail))
                    {
                        var attachParts = new List<MimeKit.MimePart>();
                        foreach (var t in tickets)
                        {
                            if (!string.IsNullOrEmpty(t.QRCodeImagePath))
                            {
                                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", t.QRCodeImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                                if (System.IO.File.Exists(path))
                                {
                                    var bytes = await System.IO.File.ReadAllBytesAsync(path);
                                    var part = new MimeKit.MimePart("image", "png")
                                    {
                                        Content = new MimeKit.MimeContent(new MemoryStream(bytes)),
                                        ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                                        ContentTransferEncoding = MimeKit.ContentEncoding.Base64,
                                        FileName = Path.GetFileName(path)
                                    };
                                    attachParts.Add(part);
                                }
                            }
                        }

                        var subject = $"Your tickets for booking #{booking.Id:D6}";
                        var placeholders = new Dictionary<string,string>
                        {
                            {"FirstName", User.Identity?.Name ?? "Customer"},
                            {"BookingNo", booking.Id.ToString("D6")},
                            {"EventName", booking.Event?.Name ?? "Event"},
                            {"EventDate", booking.Event?.Date.ToString("MMM dd, yyyy") ?? string.Empty},
                            {"TotalAmount", booking.FinalAmount.ToString("N0")}
                        };
                        await _emailService.SendTemplateAsync(customerEmail, subject, "payment-success", placeholders, attachParts);
                    }
                }
                catch { }
            }
            catch (Exception)
            {
                TempData["SuccessMessage"] = "Payment completed successfully! Your booking is confirmed.";
                TempData["WarningMessage"] = "Tickets will be generated shortly. Please check back in a few minutes.";
            }

            return RedirectToAction("Details", "Bookings", new { id = bookingId });
        }


        // GET: /Payments/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var payments = await _paymentService.GetByCustomerIdAsync(customerId);
            return View(payments);
        }

        // GET: /Payments/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var payment = await _paymentService.GetByIdAsync(id);

            if (payment == null || payment.CustomerId != customerId)
            {
                return NotFound();
            }

            return View(payment);
        }
    }
}

