using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Events.Business.Interfaces;
using Microsoft.AspNetCore.Identity;
using Star_Events.Models;
using Star_Events.Models.ViewModels;
using Star_Events.Data;
using Microsoft.EntityFrameworkCore;

namespace Star_Events.Controllers
{
	[Authorize(Roles = "Customer")]
	public class CustomerController : Controller
	{
		private readonly IEventService _eventService;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ApplicationDbContext _context;
		private readonly IPaymentService _paymentService;

		public CustomerController(IEventService eventService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IPaymentService paymentService)
		{
			_eventService = eventService;
			_userManager = userManager;
			_signInManager = signInManager;
			_context = context;
			_paymentService = paymentService;
		}

		[HttpGet]
		[AllowAnonymous]
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

		// GET: Customer/PersonalInfo
		public async Task<IActionResult> PersonalInfo()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();
			var vm = new ProfileInputViewModel
			{
				Email = user.Email ?? string.Empty,
				FirstName = user.FirstName,
				LastName = user.LastName,
				ContactNumber = user.ContactNumber
			};
			return View("~/Views/Customer/PersonalInfo.cshtml", vm);
		}

		// GET: Customer/EditPersonalInfo
		public async Task<IActionResult> EditPersonalInfo()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();
			var vm = new ProfileInputViewModel
			{
				Email = user.Email ?? string.Empty,
				FirstName = user.FirstName,
				LastName = user.LastName,
				ContactNumber = user.ContactNumber
			};
			return View("~/Views/Customer/EditPersonalInfo.cshtml", vm);
		}

		// POST: Customer/EditPersonalInfo
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPersonalInfo(ProfileInputViewModel input)
		{
			if (!ModelState.IsValid)
				return View("~/Views/Customer/EditPersonalInfo.cshtml", input);

			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			user.FirstName = input.FirstName ?? string.Empty;
			user.LastName = input.LastName ?? string.Empty;
			user.ContactNumber = input.ContactNumber ?? string.Empty;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
				return View("~/Views/Customer/EditPersonalInfo.cshtml", input);
			}
			TempData["SuccessMessage"] = "Profile updated successfully.";
			return RedirectToAction(nameof(PersonalInfo));
		}

		// GET: Customer/ChangePassword
		public IActionResult ChangePassword()
		{
			return View("~/Views/Customer/ChangePassword.cshtml", new ChangePasswordViewModel());
		}

		// POST: Customer/ChangePassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel input)
		{
			if (!ModelState.IsValid) return View("~/Views/Customer/ChangePassword.cshtml", input);
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();
			var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
			if (!result.Succeeded)
			{
				foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
				return View("~/Views/Customer/ChangePassword.cshtml", input);
			}
			await _signInManager.RefreshSignInAsync(user);
			TempData["SuccessMessage"] = "Password changed successfully.";
			return RedirectToAction(nameof(PersonalInfo));
		}

		// POST: Customer/DeleteAccount
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteAccount()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();
			var userId = user.Id;

			// Prevent delete if referenced by domain data
			var hasRefs = await _context.Bookings.AnyAsync(b => b.CustomerId == userId)
				|| await _context.Tickets.AnyAsync(t => t.CustomerId == userId)
				|| await _context.Payments.AnyAsync(p => p.CustomerId == userId)
				|| await _context.TicketSales.AnyAsync(s => s.CustomerId == userId);
			if (hasRefs)
			{
				TempData["SuccessMessage"] = "Account cannot be deleted because it is linked to bookings, tickets, or payments.";
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

	}
}
