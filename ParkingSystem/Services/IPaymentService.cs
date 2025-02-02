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
                throw new KeyNotFoundException("Payment: Reservation not found");

            if (reservation.Status != SessionStatus.Completed)
                throw new InvalidOperationException("Payment: Parking session not completed");
            if(reservation.IsPaid)
                throw new InvalidOperationException("Payment: Reservation is already paid");

            if (dto.Amount == null || dto.Amount <= 0)
                throw new InvalidOperationException("Payment: Invalid payment amount");

            var payment = new Payment
            {
                ReservationId = reservationId,
                Amount = dto.Amount,
                Method = dto.Method,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Simulate payment processing
            bool paymentSuccess = true;
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;
            if (paymentSuccess)
            {
                //update reservation paid status
                reservation.IsPaid = true;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return _mapper.Map<PaymentDto>(payment);
            }
            else
                throw new Exception("Payment: processing failed");

        }

        public async Task<PaymentDto> GetPaymentDetailsAsync(int paymentId) =>
            _mapper.Map<PaymentDto>(await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId)??
                    throw new KeyNotFoundException("Payment not found"));


        public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(int userId) =>
             _mapper.Map<IEnumerable<PaymentDto>>(await _context.Payments
                .Include(p => p.Reservation)
                .Where(p => p.Reservation.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync());

        
    }

}
