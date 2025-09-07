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

        public async Task DeleteUser(UserModel user)
        {
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync(); //soft delete
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting a user: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task EditUser(UserModel user)
        {
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating a user: {ex.Message}");
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

        public async Task<UserModel> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving user: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }
    }
}
