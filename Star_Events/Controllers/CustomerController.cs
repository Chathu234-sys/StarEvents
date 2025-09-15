using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Events.Business.Interfaces;

namespace Star_Events.Controllers
{
	[Authorize(Roles = "Customer")]
	public class CustomerController : Controller
	{
		private readonly IEventService _eventService;

		public CustomerController(IEventService eventService)
		{
			_eventService = eventService;
		}

		[HttpGet]
		public async Task<IActionResult> CustomerIndex(string? category, string? city, DateTime? date)
		{
			var events = (await _eventService.GetAllEventsAsync()).ToList();

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

			ViewBag.SelectedCategory = category;
			ViewBag.SelectedCity = city;
			ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

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

			return View("~/Views/Events/CustomerIndex.cshtml", events);
		}
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var all = await _eventService.GetAllEventsAsync();
            var ev = all.FirstOrDefault(e => e.Id == id);

            if (ev == null)
                return NotFound();

            return View("~/Views/Events/Details.cshtml", ev);
        }
    }
}
