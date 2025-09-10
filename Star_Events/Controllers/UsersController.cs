using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Star_Events.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserBusiness _business;
        public UsersController(ApplicationDbContext context , IUserBusiness business)
        {
            _context = context;
            _business = business;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = (await _business.GetAllUsers()).AsEnumerable().Where(c => c.Role == "Customer" && c.DeletedAt == null);
                if(user != null)
                {
                    return View(user);
                }
                else
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error while getting data {ex.Message}");
                throw;
            }
        }
        [ActionName("ManagerIndex")]
        public async Task<IActionResult> IndexForManagers()
        {
            try
            {
                var user = (await _business.GetAllUsers()).AsEnumerable().Where(c => c.Role == "Manager" && c.DeletedAt == null);
                if (user != null)
                {
                    return View(user);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting data {ex.Message}");
                throw;
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userModel = await _business.GetUserById(id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Age,ContactNumber,Email,Role,CreatedAt,UpdatedAt,DeletedAt")] UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                await _business.AddUsers(userModel);
                return RedirectToAction(nameof(Index));
            }
            return View(userModel);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userModel = await _business.GetUserById(id);
            if (userModel == null)
            {
                return NotFound();
            }
            return View(userModel);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Age,ContactNumber,Email,Role,CreatedAt,UpdatedAt,DeletedAt")] UserModel userModel)
        {
            if (id != userModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _business.EditUser(userModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserModelExists(userModel.Id))
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
            return View(userModel);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userModel = await _business.GetUserById(id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userModel = await _business.GetUserById(id);
            await _business.DeleteUser(userModel);
            return RedirectToAction(nameof(Index));
        }

        private bool UserModelExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
