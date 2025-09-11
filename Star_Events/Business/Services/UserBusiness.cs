using Microsoft.AspNetCore.Identity;
using Star_Events.Business.Interfaces;
using Star_Events.Data.Entities;
using Star_Events.Models;
using Star_Events.Models.ViewModels;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Business.Services
{
    public class UserBusiness : IUserBusiness
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        public UserBusiness(IUserRepository userRepository, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public Task AddUsers(UserModel user)
        {
            user.CreatedAt = DateTime.Now;
            return _userRepository.AddUsers(user);
        }

        public Task DeleteUser(UserModel user)
        {
            user.DeletedAt = DateTime.Now;
            return _userRepository.DeleteUser(user);
        }

        public Task EditUser(UserModel user)
        {
            user.UpdatedAt = DateTime.Now;
            return _userRepository.EditUser(user);
        }

        public Task<IList<UserModel>> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }

        public Task<UserModel> GetUserById(int id)
        {
            return _userRepository.GetUserById(id);
        }
    }
}
