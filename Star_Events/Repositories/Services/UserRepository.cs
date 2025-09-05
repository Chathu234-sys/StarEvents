using Microsoft.EntityFrameworkCore;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Repositories.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddUsers(UserModel user)
        {
            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while adding a user: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<IList<UserModel>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving users: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }
    }
}
