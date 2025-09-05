using Star_Events.Data.Entities;

namespace Star_Events.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task <IList<UserModel>> GetAllUsers(); // Retrieve all users
    }
}
