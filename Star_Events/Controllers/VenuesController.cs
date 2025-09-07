using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Star_Events.Data;
using Star_Events.Data.Entities;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Manager")]
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public VenuesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var venues = await _db.Venues.OrderBy(v => v.Name).ToListAsync();
            return View(venues);
        }

        public IActionResult Create() => View(new Venue());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue model)
        {
            if (!ModelState.IsValid) return View(model);
            model.Id = Guid.NewGuid();
            await _db.Venues.AddAsync(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var venue = await _db.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Venue model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _db.Venues.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}


