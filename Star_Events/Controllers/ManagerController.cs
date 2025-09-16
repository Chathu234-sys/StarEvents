using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data.Entities;
using Star_Events.Data;
using Microsoft.AspNetCore.Identity;
using Star_Events.Models;
using System.Security.Claims;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace Star_Events.Controllers
{
    [Authorize(Roles = "Manager, Admin")]
    public class ManagerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerController(IEventService eventService, ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _eventService = eventService;
            _context = context;
            _env = env;
            _userManager = userManager;
        }


        // GET: Manager/MyEvents
        public async Task<IActionResult> MyEvents()
        {

            var managerId = _userManager.GetUserId(User);
            Console.WriteLine(managerId);
            if (managerId == null) return Unauthorized();

            var events = await _eventService.GetAllEventsAsync();
            var myEvents = events.Where(e => e.ManagerId == managerId).ToList(); // Filter events by current manager
            return View(myEvents);
        }

        // GET: Manager/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // GET: Manager/Create
        public IActionResult Create()
        {
            var vm = new Star_Events.Models.ViewModels.EventCreateViewModel();
            vm.Venues = _context.Venues.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name
            }).ToList();
            return View(vm);
        }

        // POST: Manager/Create
        [HttpPost]
        public async Task<IActionResult> Create(Star_Events.Models.ViewModels.EventCreateViewModel model, IFormFile poster)
        {
            if (ModelState.IsValid)
            {
                var managerId = _userManager.GetUserId(User); // Get current manager's user ID
                if (managerId == null) return Unauthorized(); // Ensure the user is logged in
                var ev = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Date = model.Date,
                    Time = model.Time,
                    Category = model.Category,
                    Location = model.Location,
                    Description = model.Description,
                    VenueId = model.VenueId,


                    ManagerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty

                };
                ev.PosterUrl = await SavePoster(poster);
                await _eventService.CreateEventAsync(ev);

                // seed ticket types (VIP, Seating, Standing)
                var types = new List<TicketType>();
                if (model.VipTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "VIP", Price = model.VipPrice, TotalAvailable = model.VipTotal });
                if (model.RegularTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "Seating", Price = model.RegularPrice, TotalAvailable = model.RegularTotal });
                if (model.ChildrenTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "Standing", Price = model.ChildrenPrice, TotalAvailable = model.ChildrenTotal });
                if (types.Any())
                {
                    _context.TicketTypes.AddRange(types);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(MyEvents));
            }
            // reload venues on error
            model.Venues = _context.Venues.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name
            }).ToList();
            return View(model);
        }

        // GET: Manager/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // POST: Manager/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Event ev, IFormFile? poster)
        {
            if (ModelState.IsValid)
            {
                // Preserve ManagerId (not posted by the form)
                var existing = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ev.Id);
                if (existing != null)
                {
                    ev.ManagerId = existing.ManagerId;
                }
                if (poster != null)
                {
                    ev.PosterUrl = await SavePoster(poster);
                }
                await _eventService.UpdateEventAsync(ev);
                return RedirectToAction(nameof(MyEvents));
            }
            return View(ev);
        }

        // GET: Manager/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        // POST: Manager/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _eventService.DeleteEventAsync(id);
            return RedirectToAction(nameof(MyEvents));
        }

        // Helper: Save Poster
        private async Task<string?> SavePoster(IFormFile poster)
        {
            if (poster == null || poster.Length == 0) return null;

            string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "events");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(poster.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await poster.CopyToAsync(stream);
            }

            return "/images/events/" + uniqueFileName;
        }
        // Ticket Types
        public async Task<IActionResult> TicketTypes(Guid eventId)
        {
            var types = await _eventService.GetTicketTypesByEventAsync(eventId);
            ViewBag.EventId = eventId;
            return View(types);
        }

        [HttpGet]
        public IActionResult AddTicketType(Guid eventId) => View(new TicketType { EventId = eventId });

        [HttpPost]
        public async Task<IActionResult> AddTicketType(TicketType type)
        {
            if (ModelState.IsValid)
            {
                await _eventService.AddTicketTypeAsync(type);
                return RedirectToAction("TicketTypes", new { eventId = type.EventId });
            }
            return View(type);
        }

        // Sales Report
        public async Task<IActionResult> TicketSales(Guid eventId)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Get event details - ensure it belongs to current manager
            var eventDetails = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.ManagerId == managerId);

            if (eventDetails == null)
            {
                return NotFound();
            }

            // Get all ticket types for this event
            var eventTicketTypes = await _context.TicketTypes
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            // Get all sales for this event
            var sales = await _context.TicketSales
                .Where(s => s.EventId == eventId)
                .Include(s => s.TicketType)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            // Create grouped data including ticket types with zero sales
            var grouped = eventTicketTypes
                .Select(tt => new Star_Events.Models.ViewModels.TicketTypeSalesViewModel
                {
                    TicketTypeName = tt.Name,
                    Quantity = sales.Where(s => s.TicketTypeId == tt.Id).Sum(x => x.Quantity),
                    Amount = sales.Where(s => s.TicketTypeId == tt.Id).Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.TicketTypeName)
                .ToList();

            ViewBag.EventDetails = eventDetails;
            ViewBag.TotalRevenue = await _eventService.GetTotalRevenueAsync(eventId);
            ViewBag.SalesGrouped = grouped; // for charts / summary
            ViewBag.TotalTicketsSold = sales.Sum(s => s.Quantity);
            ViewBag.TotalSalesCount = sales.Count;

            // Daily revenue timeseries for line chart
            var daily = sales
                .GroupBy(s => s.SaleDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.TotalAmount) })
                .ToList();

            ViewBag.DailyLabels = string.Join(",", daily.Select(d => $"'{d.Date:MMM dd}'"));
            ViewBag.DailyAmounts = string.Join(",", daily.Select(d => d.Amount));
            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToCsv(Guid eventId)
        {
            var sales = await _context.TicketSales
                .Where(s => s.EventId == eventId)
                .Include(s => s.TicketType)
                .OrderBy(s => s.SaleDate)
                .ToListAsync();

            var lines = new List<string> { "TicketType,Quantity,TotalAmount,SaleDate" };
            lines.AddRange(sales.Select(s => $"{EscapeCsv(s.TicketType.Name)},{s.Quantity},{s.TotalAmount},{s.SaleDate:yyyy-MM-dd}"));
            var bytes = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
            return File(bytes, "text/csv", "ticket-sales.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(Guid eventId)
        {
            var sales = await _context.TicketSales
                .Where(s => s.EventId == eventId)
                .Include(s => s.TicketType)
                .OrderBy(s => s.SaleDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Sales");
            ws.Cell(1, 1).Value = "Ticket Type";
            ws.Cell(1, 2).Value = "Quantity";
            ws.Cell(1, 3).Value = "Total Amount";
            ws.Cell(1, 4).Value = "Sale Date";
            int row = 2;
            foreach (var s in sales)
            {
                ws.Cell(row, 1).Value = s.TicketType.Name;
                ws.Cell(row, 2).Value = s.Quantity;
                ws.Cell(row, 3).Value = s.TotalAmount;
                ws.Cell(row, 4).Value = s.SaleDate.ToString("yyyy-MM-dd");
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ticket-sales.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf(Guid eventId)
        {
            var sales = await _context.TicketSales
                .Where(s => s.EventId == eventId)
                .Include(s => s.TicketType)
                .OrderBy(s => s.SaleDate)
                .ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Ticket Sales Report").SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(90);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Ticket Type");
                            header.Cell().Element(CellStyle).Text("Quantity");
                            header.Cell().Element(CellStyle).Text("Total Amount");
                            header.Cell().Element(CellStyle).Text("Sale Date");
                        });

                        foreach (var s in sales)
                        {
                            table.Cell().Element(CellStyle).Text(s.TicketType.Name);
                            table.Cell().Element(CellStyle).Text(s.Quantity.ToString());
                            table.Cell().Element(CellStyle).Text($"Rs. {s.TotalAmount:N0}");
                            table.Cell().Element(CellStyle).Text(s.SaleDate.ToString("yyyy-MM-dd"));
                        }

                        IContainer CellStyle(IContainer c) => c.Border(0.5f).Padding(4);
                    });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            var bytes = ms.ToArray();
            return File(bytes, "application/pdf", "ticket-sales.pdf");
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (input.Contains(',') || input.Contains('"'))
                return '"' + input.Replace("\"", "\"\"") + '"';
            return input;
        }
    }
}
