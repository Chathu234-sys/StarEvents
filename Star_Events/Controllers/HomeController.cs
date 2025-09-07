using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Events.Models;
using Star_Events.Business.Interfaces;
using Star_Events.Data.Entities;

namespace Star_Events.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEventService _eventService;

    public HomeController(ILogger<HomeController> logger, IEventService eventService)
    {
        _logger = logger;
        _eventService = eventService;
    }

    public async Task<IActionResult> Index(string? category, string? city, DateTime? date)
    {
        var events = (await _eventService.GetAllEventsAsync()).ToList();

        // Build filter sources
        ViewBag.Categories = events.Select(e => e.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();
        ViewBag.Cities = events.Select(e => e.Location).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).ToList();

        // Persist filter values
        ViewBag.SelectedCategory = category;
        ViewBag.SelectedCity = city;
        ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

        // Apply filters
        if (!string.IsNullOrWhiteSpace(category))
        {
            events = events.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(city))
        {
            events = events.Where(e => string.Equals(e.Location, city, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (date.HasValue)
        {
            var d = date.Value.Date;
            events = events.Where(e => e.Date.Date == d).ToList();
        }

        return View(events);
    }

    //[Authorize (Roles = "Admin")]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
