using Star_Events.Data.Entities;

namespace Star_Events.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(string customerId);
        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task SaveAsync();
    }
}


