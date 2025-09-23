using Microsoft.EntityFrameworkCore;
using Star_Events.Business.Interfaces;
using Star_Events.Data;
using Star_Events.Data.Entities;
using Star_Events.Repositories.Interfaces;

namespace Star_Events.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ApplicationDbContext _context;

        public PaymentService(IPaymentRepository paymentRepository, IBookingRepository bookingRepository, ApplicationDbContext context)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<Payment?> GetByPaymentIdAsync(string paymentId)
        {
            return await _paymentRepository.GetByPaymentIdAsync(paymentId);
        }

        public async Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId)
        {
            return await _paymentRepository.GetByBookingIdAsync(bookingId);
        }

        public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(string customerId)
        {
            return await _paymentRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _paymentRepository.GetByStatusAsync(status);
        }

        public async Task<Payment> CreatePaymentAsync(int bookingId, string customerId, decimal amount, PaymentMethod method, string? transactionId = null)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("Booking is not in pending status");

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid().ToString("N"),
                BookingId = bookingId,
                CustomerId = customerId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                TransactionId = transactionId,
                PaymentDetails = $"Payment for booking #{booking.BookingNumber}",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return await _paymentRepository.AddAsync(payment);
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            return await _paymentRepository.UpdateAsync(payment);
        }

        public async Task<Payment> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string? failureReason = null)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            if (status == PaymentStatus.Completed)
            {
                payment.ProcessedAt = DateTime.UtcNow;
                
                // Update booking status
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                    booking.PaymentDate = DateTime.UtcNow;
                    await _bookingRepository.UpdateAsync(booking);
                }
            }
            else if (status == PaymentStatus.Failed)
            {
                payment.FailureReason = failureReason;
            }

            return await _paymentRepository.UpdateAsync(payment);
        }

        public async Task<bool> ProcessPaymentAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null || payment.Status != PaymentStatus.Pending)
                return false;

            try
            {
                payment.Status = PaymentStatus.Processing;
                await _paymentRepository.UpdateAsync(payment);

                // Simulate payment processing
                await Task.Delay(1000);

                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                // Update booking status
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                    booking.PaymentDate = DateTime.UtcNow;
                    await _bookingRepository.UpdateAsync(booking);
                }

                return true;
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = ex.Message;
                await _paymentRepository.UpdateAsync(payment);
                return false;
            }
        }


        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.IsActive)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetRevenueByEventAsync(Guid eventId)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
                           p.IsActive && 
                           p.Booking.EventId == eventId)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetRevenueByCustomerAsync(string customerId)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
                           p.IsActive && 
                           p.CustomerId == customerId)
                .SumAsync(p => p.Amount);
        }
    }
}

