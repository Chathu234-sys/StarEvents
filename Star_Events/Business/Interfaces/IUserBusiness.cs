using Star_Events.Data.Entities;

namespace Star_Events.Business.Interfaces
{
    public interface IUserBusiness
    {
        Task<IList<UserModel>> GetAllUsers(); // Retrieve all users
    }
}
