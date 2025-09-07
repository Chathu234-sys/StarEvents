using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data.Entities;
using Star_Events.Data;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ManagerController(IEventService eventService, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _eventService = eventService;
            _context = context;
            _env = env;
        }

        // GET: Manager/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
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
                var ev = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Date = model.Date,
                    Time = model.Time,
                    Category = model.Category,
                    Location = model.Location,
                    Description = model.Description,
                    VenueId = model.VenueId
                };
                ev.PosterUrl = await SavePoster(poster);
                await _eventService.CreateEventAsync(ev);

                // seed ticket types
                var types = new List<TicketType>();
                if (model.VipTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "VIP", Price = model.VipPrice, TotalAvailable = model.VipTotal });
                if (model.RegularTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "Regular", Price = model.RegularPrice, TotalAvailable = model.RegularTotal });
                if (model.ChildrenTotal > 0) types.Add(new TicketType { Id = Guid.NewGuid(), EventId = ev.Id, Name = "Children", Price = model.ChildrenPrice, TotalAvailable = model.ChildrenTotal });
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
            var sales = await _context.TicketSales
                .Where(s => s.TicketType.EventId == eventId)
                .Include(s => s.TicketType)
                .ToListAsync();

            ViewBag.TotalRevenue = await _eventService.GetTotalRevenueAsync(eventId);
            return View(sales);
        }
    }
}
