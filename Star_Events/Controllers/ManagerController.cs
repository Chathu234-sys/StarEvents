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
using Star_Events.Models.ViewModels;


namespace Star_Events.Controllers
{
    [Authorize(Roles = "Manager, Admin")]
    public class ManagerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ManagerController(IEventService eventService, ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _eventService = eventService;
            _context = context;
            _env = env;
            _userManager = userManager;
            _signInManager = signInManager;
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

        // Personal Info (Manager)
        public async Task<IActionResult> PersonalInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var vm = new Star_Events.Models.ViewModels.ProfileInputViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ContactNumber = user.ContactNumber
            };
            return View("~/Views/Manager/PersonalInfo.cshtml", vm);
        }

        public async Task<IActionResult> EditPersonalInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var vm = new Star_Events.Models.ViewModels.ProfileInputViewModel
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ContactNumber = user.ContactNumber
            };
            return View("~/Views/Manager/EditPersonalInfo.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonalInfo(ProfileInputViewModel input)
        {
            if (!ModelState.IsValid) return View("~/Views/Manager/EditPersonalInfo.cshtml", input);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            user.FirstName = input.FirstName ?? string.Empty;
            user.LastName = input.LastName ?? string.Empty;
            user.ContactNumber = input.ContactNumber ?? string.Empty;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("~/Views/Manager/EditPersonalInfo.cshtml", input);
            }
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(PersonalInfo));
        }

        // Change Password (Manager)
        public IActionResult ChangePassword()
        {
            return View("~/Views/Manager/ChangePassword.cshtml", new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel input)
        {
            if (!ModelState.IsValid) return View("~/Views/Manager/ChangePassword.cshtml", input);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("~/Views/Manager/ChangePassword.cshtml", input);
            }
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(PersonalInfo));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var userId = user.Id;

            // Block delete if manages events
            var managesEvents = await _context.Events.AnyAsync(e => e.ManagerId == userId);
            if (managesEvents)
            {
                TempData["SuccessMessage"] = "Account cannot be deleted while you have events. Delete/transfer events first.";
                return RedirectToAction(nameof(PersonalInfo));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["SuccessMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(PersonalInfo));
            }
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Your account has been deleted.";
            return RedirectToAction("Index", "Home");
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
            // Check for duplicate event name and date
            if (await _context.Events.AnyAsync(e => e.Name == model.Name && e.Date == model.Date))
            {
                ModelState.AddModelError("Name", "An event with this name and date already exists.");
            }

            // Check for at least one valid ticket type
            bool hasValidTicketType = (model.VipPrice > 0 && model.VipTotal > 0) ||
                                    (model.SeatingPrice > 0 && model.SeatingTotal > 0) ||
                                    (model.StandingPrice > 0 && model.StandingTotal > 0);

            if (!hasValidTicketType)
            {
                ModelState.AddModelError("", "At least one ticket type must be configured with both price and quantity greater than 0.");
            }

            if (ModelState.IsValid)
            {
                var managerId = _userManager.GetUserId(User);
                if (managerId == null) return Unauthorized();

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
                    ManagerId = managerId
                };
                ev.PosterUrl = await SavePoster(poster);
                await _eventService.CreateEventAsync(ev);

                // Ticket types
                var types = new List<TicketType>();
                if (model.VipTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "VIP", Price = model.VipPrice, TotalAvailable = model.VipTotal });
                if (model.SeatingTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "Seating", Price = model.SeatingPrice, TotalAvailable = model.SeatingTotal });
                if (model.StandingTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "Standing", Price = model.StandingPrice, TotalAvailable = model.StandingTotal });
                if (types.Any())
                {
                    _context.TicketTypes.AddRange(types);
                    await _context.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(MyEvents));
            }

            // reload venues if validation fails
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
            
            // Load venues for dropdown
            ViewBag.Venues = _context.Venues.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name
            }).ToList();
            
            return View(ev);
        }

        // POST: Manager/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Event ev, IFormFile? poster)
        {
            // Check for duplicate event name and date (excluding current event)
            if (await _context.Events.AnyAsync(e => e.Name == ev.Name && e.Date == ev.Date && e.Id != ev.Id))
            {
                ModelState.AddModelError("Name", "An event with this name and date already exists.");
            }

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
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(MyEvents));
            }
            
            // Reload venues if validation fails
            ViewBag.Venues = _context.Venues.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.Name
            }).ToList();
            
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
            try
            {
                await _eventService.DeleteEventAsync(id);
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting event: {ex.Message}";
            }
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
            
            TempData["SuccessMessage"] = "CSV report downloaded successfully!";
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
            
            TempData["SuccessMessage"] = "Excel report downloaded successfully!";
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
            
            TempData["SuccessMessage"] = "PDF report downloaded successfully!";
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
