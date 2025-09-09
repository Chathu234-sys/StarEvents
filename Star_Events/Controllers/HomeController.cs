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

    public async Task<IActionResult> Index()
    {
        // Get upcoming events (events from today onwards)
        var allEvents = (await _eventService.GetAllEventsAsync()).ToList();
        var upcomingEvents = allEvents
            .Where(e => e.Date >= DateTime.Today)
            .OrderBy(e => e.Date)
            .Take(6) // Show only 6 upcoming events on home page
            .ToList();

        ViewBag.UpcomingEvents = upcomingEvents;
        return View();
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
