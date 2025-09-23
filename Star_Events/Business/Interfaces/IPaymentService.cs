using Star_Events.Data.Entities;

namespace Star_Events.Business.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByPaymentIdAsync(string paymentId);
        Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<Payment> CreatePaymentAsync(int bookingId, string customerId, decimal amount, PaymentMethod method, string? transactionId = null);
        Task<Payment> UpdateAsync(Payment payment);
        Task<Payment> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string? failureReason = null);
        Task<bool> ProcessPaymentAsync(int paymentId);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByEventAsync(Guid eventId);
        Task<decimal> GetRevenueByCustomerAsync(string customerId);
    }

}

