using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.ReservationDtos;

namespace ParkingSystem.Services
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(int userId, CreateReservationDto dto);
        Task<ReservationDto> StartParkingAsync(string qrCode);
        Task<ReservationDto> EndParkingAsync(string qrCode);
        Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId);
        Task<decimal> CalculateParkingFeeAsync(int reservationId);
    }
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IQRCodeService _qrCodeService;

        public ReservationService(AppDbContext context, IMapper mapper,IQRCodeService qrCodeService)
        {
            _context = context;
            _mapper = mapper;
            _qrCodeService = qrCodeService;
        }

        public async Task<ReservationDto> CreateReservationAsync(int userId, CreateReservationDto dto)
        {
            // Verify car belongs to user
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == dto.CarId && c.UserId == userId);

            if (car == null)
                throw new InvalidOperationException("Car not found or doesn't belong to user");

            // Verify spot is available
            var spot = await _context.ParkingSpots
                .FirstOrDefaultAsync(s => s.Id == dto.ParkingSpotId);

            if (spot == null || spot.Status != SpotStatus.Available)
                throw new InvalidOperationException("Parking spot not available");

            // Create reservation
            var reservation = new Reservation
            {
                UserId = userId,
                CarId = dto.CarId,
                ParkingSpotId = dto.ParkingSpotId,
                EntryTime = DateTime.UtcNow,
                Status = SessionStatus.Active
            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            reservation.QRCode = _qrCodeService.GenerateQRCode(
                     reservation.Id,
                     userId,
                     DateTime.UtcNow
                 );


            // Update spot status
            spot.Status = SpotStatus.Occupied;
            spot.ReservationId = reservation.Id;

            await _context.SaveChangesAsync();

            return await GetReservationDtoAsync(reservation.Id);
        }

        public async Task<ReservationDto> StartParkingAsync(string qrCode)
        {
            if (!_qrCodeService.ValidateQRCode(qrCode))
                throw new InvalidOperationException("Invalid QR code");

            var (reservationId, userId) = _qrCodeService.DecodeQRCode(qrCode);

            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");


            if (reservation.Status != SessionStatus.Active)
                throw new InvalidOperationException("Reservation is not active");

            reservation.EntryTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto> EndParkingAsync(string qrCode)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.QRCode == qrCode);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            reservation.ExitTime = DateTime.UtcNow;
            reservation.TotalAmount = await CalculateParkingFeeAsync(reservation.Id);
            reservation.Status = SessionStatus.Completed;

            // Free up the parking spot
            var spot = reservation.ParkingSpot;
            spot.Status = SpotStatus.Available;
            spot.ReservationId = null;

            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        private string GenerateQRCode()
        {
            return Guid.NewGuid().ToString("N");
        }

        private async Task<ReservationDto> GetReservationDtoAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<decimal> CalculateParkingFeeAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            var exitTime = reservation.ExitTime ?? DateTime.UtcNow;
            var duration = exitTime - reservation.EntryTime;

            // Example pricing: $2 per hour, minimum 1 hour
            var hours = Math.Ceiling(duration.TotalHours);
            return (decimal)hours * 2.0m;
        }

        public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.ParkingSpot)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.EntryTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReservationDto>>(reservations);
        }
    }
}
