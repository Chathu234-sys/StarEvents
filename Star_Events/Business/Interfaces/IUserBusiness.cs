using Star_Events.Data.Entities;
using Star_Events.Models.ViewModels;

namespace Star_Events.Business.Interfaces
{
    public interface IUserBusiness
    {
        Task<IList<UserModel>> GetAllUsers(); // Retrieve all users
        Task AddUsers(UserModel user); // Add a new user
        Task<UserModel> GetUserById(int id); //Get user details from id
        Task EditUser(UserModel user); //Edit users
        Task DeleteUser(UserModel user); //Delete user
    }
}
