using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Star_Events.Data;
using Star_Events.Data.Entities;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Events.Include(e => e.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Events/OutdatedEvents
        [ActionName("OutdatedEvents")]
        public async Task<IActionResult> OutdatedEventsIndex()
        {
            var applicationDbContext = _context.Events
                .Include(e => e.Venue)
                .Where(e => e.Date < DateTime.Today);
            return View(await applicationDbContext.ToListAsync());
        }

        [ActionName("OngoingEvents")]
        public async Task<IActionResult> OngoingEventsIndex()
        {
            var applicationDbContext = _context.Events
                .Include(e => e.Venue)
                .Where(e => e.Date >= DateTime.Today);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Events/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.TicketTypes)   //  load ticket types, not just sales
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Date,Time,Category,Location,Description,TicketPrice,PosterUrl,VenueId,ManagerId")] Event @event)
        {
            // Check for duplicate event name and date
            if (await _context.Events.AnyAsync(e => e.Name == @event.Name && e.Date == @event.Date))
            {
                ModelState.AddModelError("Name", "An event with this name and date already exists.");
            }

            if (ModelState.IsValid)
            {
                @event.Id = Guid.NewGuid();
                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Date,Time,Category,Location,Description,TicketPrice,PosterUrl,VenueId,ManagerId")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            // Check for duplicate event name and date (excluding current event)
            if (await _context.Events.AnyAsync(e => e.Name == @event.Name && e.Date == @event.Date && e.Id != @event.Id))
            {
                ModelState.AddModelError("Name", "An event with this name and date already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var @event = await _context.Events.FindAsync(id);
                if (@event != null)
                {
                    _context.Events.Remove(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting event: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(Guid id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}


