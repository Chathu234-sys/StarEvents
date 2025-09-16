using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Star_Events.Data;
using System.Globalization;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard with summary and charts
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalEvents = await _context.Events.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalManagers = await _context.Users.CountAsync(u => u.Role == "Manager");
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
            var totalBookings = await _context.Bookings.CountAsync();
            var totalRevenue = await _context.TicketSales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

            // Daily revenue for last 30 days
            var start = DateTime.UtcNow.Date.AddDays(-29);
            var daily = await _context.TicketSales
                .Where(s => s.SaleDate.Date >= start)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.DailyLabels = string.Join(",", Enumerable.Range(0, 30)
                .Select(i => start.AddDays(i))
                .Select(d => $"'{d:MMM dd}'"));
            var amountMap = daily.ToDictionary(d => d.Date, d => d.Amount);
            ViewBag.DailyAmounts = string.Join(",", Enumerable.Range(0, 30)
                .Select(i => start.AddDays(i))
                .Select(d => amountMap.TryGetValue(d, out var val) ? val : 0m));

            // Users by role pie
            ViewBag.UserRoleLabels = "'Customers','Managers','Admins'";
            var admins = await _context.Users.CountAsync(u => u.Role == "Admin");
            ViewBag.UserRoleData = string.Join(",", new[] { totalCustomers, totalManagers, admins });

            // Revenue by event (top 6)
            var topEvents = await _context.TicketSales
                .GroupBy(s => s.EventId)
                .Select(g => new { EventId = g.Key, Amount = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.Amount)
                .Take(6)
                .Join(_context.Events, x => x.EventId, e => e.Id, (x, e) => new { e.Name, x.Amount })
                .ToListAsync();

            ViewBag.EventRevenueLabels = string.Join(",", topEvents.Select(t => $"'{t.Name}'"));
            ViewBag.EventRevenueData = string.Join(",", topEvents.Select(t => t.Amount));

            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalManagers = totalManagers;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalRevenue = totalRevenue;
            return View();
        }

        // Users report
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.OrderBy(u => u.Role).ThenBy(u => u.FirstName).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UsersExcel()
        {
            var users = await _context.Users.OrderBy(u => u.Role).ThenBy(u => u.FirstName).ToListAsync();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Users");
            ws.Cell(1, 1).Value = "First Name";
            ws.Cell(1, 2).Value = "Last Name";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Role";
            int r = 2;
            foreach (var u in users)
            {
                ws.Cell(r, 1).Value = u.FirstName;
                ws.Cell(r, 2).Value = u.LastName;
                ws.Cell(r, 3).Value = u.Email;
                ws.Cell(r, 4).Value = u.Role;
                r++;
            }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> UsersPdf()
        {
            var users = await _context.Users.OrderBy(u => u.Role).ThenBy(u => u.FirstName).ToListAsync();
            QuestPDF.Settings.License = LicenseType.Community;
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Users Report").SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                    });
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.ConstantColumn(90); });
                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("First Name");
                            h.Cell().Element(HeaderCell).Text("Last Name");
                            h.Cell().Element(HeaderCell).Text("Email");
                            h.Cell().Element(HeaderCell).Text("Role");
                        });
                        foreach (var u in users)
                        {
                            table.Cell().Element(Cell).Text(u.FirstName);
                            table.Cell().Element(Cell).Text(u.LastName);
                            table.Cell().Element(Cell).Text(u.Email);
                            table.Cell().Element(Cell).Text(u.Role);
                        }
                        static IContainer HeaderCell(IContainer c) => c.Background(Colors.Grey.Lighten3).Border(0.75f).Padding(6);
                        static IContainer Cell(IContainer c) => c.Border(0.5f).Padding(6);
                    });
                    page.Footer().AlignRight().Text(txt =>
                    {
                        txt.Span("Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });
            });
            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return File(ms.ToArray(), "application/pdf", "users.pdf");
        }

        // Events report
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var events = await _context.Events
                .Select(e => new {
                    e.Id, e.Name, e.Date, e.Location,
                    Revenue = _context.TicketSales.Where(s => s.EventId == e.Id).Sum(s => (decimal?)s.TotalAmount) ?? 0m,
                    Tickets = _context.TicketSales.Where(s => s.EventId == e.Id).Sum(s => (int?)s.Quantity) ?? 0
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();
            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> EventsExcel()
        {
            var data = await _context.Events
                .Select(e => new {
                    e.Name, e.Date, e.Location,
                    Revenue = _context.TicketSales.Where(s => s.EventId == e.Id).Sum(s => (decimal?)s.TotalAmount) ?? 0m,
                    Tickets = _context.TicketSales.Where(s => s.EventId == e.Id).Sum(s => (int?)s.Quantity) ?? 0
                })
                .OrderByDescending(x => x.Date).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Events");
            ws.Cell(1, 1).Value = "Event";
            ws.Cell(1, 2).Value = "Date";
            ws.Cell(1, 3).Value = "Location";
            ws.Cell(1, 4).Value = "Tickets Sold";
            ws.Cell(1, 5).Value = "Revenue";
            int r = 2;
            foreach (var e in data)
            {
                ws.Cell(r, 1).Value = e.Name;
                ws.Cell(r, 2).Value = e.Date.ToString("yyyy-MM-dd");
                ws.Cell(r, 3).Value = e.Location;
                ws.Cell(r, 4).Value = e.Tickets;
                ws.Cell(r, 5).Value = e.Revenue;
                r++;
            }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "events.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> EventsPdf()
        {
            var data = await _context.Events
                .Select(e => new {
                    e.Name, e.Date, e.Location,
                    Revenue = _context.TicketSales.Where(s => s.EventId == e.Id).Sum(s => (decimal?)s.TotalAmount) ?? 0m,
                    Tickets = _context.TicketSales.Where(s => s.EventId == e.Id).Sum(s => (int?)s.Quantity) ?? 0
                })
                .OrderByDescending(x => x.Date).ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;
            var doc = Document.Create(container =>
            {
                container.Page(p =>
                {
                    p.Margin(25);
                    p.Header().Column(col =>
                    {
                        col.Item().Text("Events Report").SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    });
                    p.Content().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(90); c.RelativeColumn(); c.ConstantColumn(90); c.ConstantColumn(90); });
                        t.Header(h =>
                        {
                            h.Cell().Element(HeaderCell).Text("Event");
                            h.Cell().Element(HeaderCell).Text("Date");
                            h.Cell().Element(HeaderCell).Text("Location");
                            h.Cell().Element(HeaderCell).Text("Tickets");
                            h.Cell().Element(HeaderCell).Text("Revenue");
                        });
                        foreach (var e in data)
                        {
                            t.Cell().Element(Cell).Text(e.Name);
                            t.Cell().Element(Cell).Text(e.Date.ToString("yyyy-MM-dd"));
                            t.Cell().Element(Cell).Text(e.Location);
                            t.Cell().Element(c => Cell(c).AlignRight()).Text(e.Tickets.ToString());
                            t.Cell().Element(c => Cell(c).AlignRight()).Text($"Rs. {e.Revenue:N0}");
                        }
                        static IContainer HeaderCell(IContainer c) => c.Background(Colors.Grey.Lighten3).Border(0.75f).Padding(6);
                        static IContainer Cell(IContainer c) => c.Border(0.5f).Padding(6);
                    });
                    p.Footer().AlignRight().Text(txt => { txt.Span("Page "); txt.CurrentPageNumber(); txt.Span(" / "); txt.TotalPages(); });
                });
            });
            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return File(ms.ToArray(), "application/pdf", "events.pdf");
        }

        // Bookings report
        [HttpGet]
        public async Task<IActionResult> Bookings(Guid? eventId)
        {
            // Event selector
            var events = await _context.Events
                .OrderByDescending(e => e.Date)
                .Select(e => new { e.Id, e.Name })
                .ToListAsync();

            if (events.Count == 0)
            {
                ViewBag.Events = new List<object>();
                ViewBag.SelectedEventId = null;
                return View(Enumerable.Empty<Data.Entities.Booking>());
            }

            var selectedId = eventId ?? events.First().Id;
            ViewBag.Events = events;
            ViewBag.SelectedEventId = selectedId;

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.EventId == selectedId)
                .OrderByDescending(b => b.BookingDate)
                .Take(1000)
                .ToListAsync();
            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> BookingsExcel(Guid? eventId)
        {
            var query = _context.Bookings.Include(b => b.Event).AsQueryable();
            if (eventId.HasValue) query = query.Where(b => b.EventId == eventId.Value);
            var bookings = await query.OrderByDescending(b => b.BookingDate).ToListAsync();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Bookings");
            ws.Cell(1, 1).Value = "Booking#";
            ws.Cell(1, 2).Value = "Event";
            ws.Cell(1, 3).Value = "Date";
            ws.Cell(1, 4).Value = "Status";
            ws.Cell(1, 5).Value = "Amount";
            int r = 2;
            foreach (var b in bookings)
            {
                ws.Cell(r, 1).Value = b.Id;
                ws.Cell(r, 2).Value = b.Event?.Name ?? string.Empty;
                ws.Cell(r, 3).Value = b.BookingDate.ToString("yyyy-MM-dd");
                ws.Cell(r, 4).Value = b.Status.ToString();
                ws.Cell(r, 5).Value = b.FinalAmount;
                r++;
            }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var fileName = eventId.HasValue ? $"bookings_{eventId.Value}.xlsx" : "bookings.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> BookingsPdf(Guid? eventId)
        {
            var query = _context.Bookings.Include(b => b.Event).AsQueryable();
            if (eventId.HasValue) query = query.Where(b => b.EventId == eventId.Value);
            var bookings = await query.OrderByDescending(b => b.BookingDate).ToListAsync();
            var eventName = eventId.HasValue ? (await _context.Events.Where(e => e.Id == eventId).Select(e => e.Name).FirstOrDefaultAsync()) : "All Events";
            QuestPDF.Settings.License = LicenseType.Community;
            var doc = Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(25);
                    p.Header().Column(col =>
                    {
                        col.Item().Text("Bookings Report").SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);
                        col.Item().Text(eventName).FontSize(12).FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    });
                    p.Content().Table(t =>
                    {
                        t.ColumnsDefinition(cols => { cols.ConstantColumn(60); cols.RelativeColumn(); cols.ConstantColumn(90); cols.ConstantColumn(80); cols.ConstantColumn(90); });
                        t.Header(h => { h.Cell().Element(HeaderCell).Text("#"); h.Cell().Element(HeaderCell).Text("Event"); h.Cell().Element(HeaderCell).Text("Date"); h.Cell().Element(HeaderCell).Text("Status"); h.Cell().Element(HeaderCell).Text("Amount"); });
                        foreach (var b in bookings)
                        {
                            t.Cell().Element(Cell).Text(b.Id.ToString());
                            t.Cell().Element(Cell).Text(b.Event?.Name ?? string.Empty);
                            t.Cell().Element(Cell).Text(b.BookingDate.ToString("yyyy-MM-dd"));
                            t.Cell().Element(Cell).Text(b.Status.ToString());
                            t.Cell().Element(c => Cell(c).AlignRight()).Text($"Rs. {b.FinalAmount:N0}");
                        }
                        static IContainer HeaderCell(IContainer c) => c.Background(Colors.Grey.Lighten3).Border(0.75f).Padding(6);
                        static IContainer Cell(IContainer c) => c.Border(0.5f).Padding(6);
                    });
                    p.Footer().AlignRight().Text(txt => { txt.Span("Page "); txt.CurrentPageNumber(); txt.Span(" / "); txt.TotalPages(); });
                });
            });
            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            var fileName = eventId.HasValue ? $"bookings_{eventId.Value}.pdf" : "bookings.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }

        // Sales report (all events)
        [HttpGet]
        public async Task<IActionResult> Sales(Guid? eventId)
        {
            var events = await _context.Events.OrderByDescending(e => e.Date).Select(e => new { e.Id, e.Name }).ToListAsync();
            ViewBag.Events = events;
            ViewBag.SelectedEventId = eventId;

            var query = _context.TicketSales.Include(s => s.TicketType).AsQueryable();
            if (eventId.HasValue)
                query = query.Where(s => s.EventId == eventId.Value);

            var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync();
            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> SalesExcel(Guid? eventId)
        {
            var query = _context.TicketSales.Include(s => s.TicketType).AsQueryable();
            if (eventId.HasValue) query = query.Where(s => s.EventId == eventId.Value);
            var sales = await query.OrderBy(s => s.SaleDate).ToListAsync();
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Sales");
            ws.Cell(1, 1).Value = "Ticket Type";
            ws.Cell(1, 2).Value = "Quantity";
            ws.Cell(1, 3).Value = "Total Amount";
            ws.Cell(1, 4).Value = "Sale Date";
            int r = 2;
            foreach (var s in sales)
            {
                ws.Cell(r, 1).Value = s.TicketType.Name;
                ws.Cell(r, 2).Value = s.Quantity;
                ws.Cell(r, 3).Value = s.TotalAmount;
                ws.Cell(r, 4).Value = s.SaleDate.ToString("yyyy-MM-dd");
                r++;
            }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var excelName = eventId.HasValue ? $"sales_{eventId.Value}.xlsx" : "sales.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [HttpGet]
        public async Task<IActionResult> SalesPdf(Guid? eventId)
        {
            var query = _context.TicketSales.Include(s => s.TicketType).AsQueryable();
            if (eventId.HasValue) query = query.Where(s => s.EventId == eventId.Value);
            var sales = await query.OrderBy(s => s.SaleDate).ToListAsync();
            QuestPDF.Settings.License = LicenseType.Community;
            var doc = Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(25);
                    p.Header().Column(col =>
                    {
                        col.Item().Text("Sales Report").SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    });
                    p.Content().Table(t =>
                    {
                        t.ColumnsDefinition(cols => { cols.RelativeColumn(); cols.ConstantColumn(80); cols.ConstantColumn(110); cols.ConstantColumn(90); });
                        t.Header(h => { h.Cell().Element(HeaderCell).Text("Ticket Type"); h.Cell().Element(HeaderCell).Text("Qty"); h.Cell().Element(HeaderCell).Text("Amount"); h.Cell().Element(HeaderCell).Text("Date"); });
                        foreach (var s in sales)
                        {
                            t.Cell().Element(Cell).Text(s.TicketType.Name);
                            t.Cell().Element(c => Cell(c).AlignRight()).Text(s.Quantity.ToString());
                            t.Cell().Element(c => Cell(c).AlignRight()).Text($"Rs. {s.TotalAmount:N0}");
                            t.Cell().Element(Cell).Text(s.SaleDate.ToString("yyyy-MM-dd"));
                        }
                        static IContainer HeaderCell(IContainer c) => c.Background(Colors.Grey.Lighten3).Border(0.75f).Padding(6);
                        static IContainer Cell(IContainer c) => c.Border(0.5f).Padding(6);
                    });
                    p.Footer().AlignRight().Text(txt => { txt.Span("Page "); txt.CurrentPageNumber(); txt.Span(" / "); txt.TotalPages(); });
                });
            });
            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            var pdfName = eventId.HasValue ? $"sales_{eventId.Value}.pdf" : "sales.pdf";
            return File(ms.ToArray(), "application/pdf", pdfName);
        }
    }
}


