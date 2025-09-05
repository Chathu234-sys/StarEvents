using Star_Events.Business.Interfaces;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Business.Services
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserRepository _userRepository;
        public UserBusiness(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public Task<IList<UserModel>> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }
    }
}
