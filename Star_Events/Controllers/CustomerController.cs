using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Events.Business.Interfaces;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Customer")] //  All actions require Customer login
    public class CustomerController : Controller
    {
        private readonly IEventService _eventService;

        public CustomerController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? city, DateTime? date)
        {
            var events = (await _eventService.GetAllEventsAsync()).ToList();

            // Populate dropdowns
            ViewBag.Categories = events.Select(e => e.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Cities = events.Select(e => e.Location)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Remember selected filters
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedCity = city;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

            // Apply filters
            if (!string.IsNullOrWhiteSpace(category))
            {
                events = events
                    .Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                events = events
                    .Where(e => string.Equals(e.Location, city, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (date.HasValue)
            {
                var d = date.Value.Date;
                events = events
                    .Where(e => e.Date.Date == d)
                    .ToList();
            }

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var all = await _eventService.GetAllEventsAsync();
            var ev = all.FirstOrDefault(e => e.Id == id);

            if (ev == null)
                return NotFound();

            return View(ev);
        }
    }
}
