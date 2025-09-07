using Star_Events.Data.Entities;

namespace Star_Events.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task <IList<UserModel>> GetAllUsers(); // Retrieve all users
        Task AddUsers(UserModel user); // Add a new user
        Task<UserModel> GetUserById(int id); //Get user details from id
        Task EditUser(UserModel user); //Edit users
        Task DeleteUser(UserModel user); //Delete user
    }
}
