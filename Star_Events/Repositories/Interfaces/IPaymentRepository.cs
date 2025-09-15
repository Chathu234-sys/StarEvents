using Star_Events.Data.Entities;

namespace Star_Events.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByPaymentIdAsync(string paymentId);
        Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<Payment> AddAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(int id);
        Task SaveAsync();
    }
}

