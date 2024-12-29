using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.PaymentDtos;

namespace ParkingSystem.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto> ProcessPaymentAsync(int reservationId, ProcessPaymentDto dto);
        Task<PaymentDto> GetPaymentDetailsAsync(int paymentId);
        Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(int userId);
    }
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PaymentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDto> ProcessPaymentAsync(int reservationId, ProcessPaymentDto dto)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            if (reservation.Status != SessionStatus.Completed)
                throw new InvalidOperationException("Parking session not completed");

            var payment = new Payment
            {
                ReservationId = reservationId,
                Amount = dto.Amount,
                Method = dto.Method,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Simulate payment processing
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> GetPaymentDetailsAsync(int paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new KeyNotFoundException("Payment not found");

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(int userId)
        {
            var payments = await _context.Payments
                .Include(p => p.Reservation)
                .Where(p => p.Reservation.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }
    }

}
