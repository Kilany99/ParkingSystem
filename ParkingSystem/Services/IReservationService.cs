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
        Task<ReservationDto> CancelReservationAsync(int reservationId, int userId);

        Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(int userId);
        Task<decimal> CalculateParkingFeeAsync(int reservationId);
        public Task<ReservationDto> GetActiveReservation(int carId);
        Task<bool> HasActiveReservation(int carId);
    }
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IQRCodeService _qrCodeService;
        private readonly IParkingZoneService _parkingZoneService;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(AppDbContext context, IMapper mapper,IQRCodeService qrCodeService,
            IParkingZoneService parkingZoneService, ILogger<ReservationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _qrCodeService = qrCodeService;
            _parkingZoneService = parkingZoneService;
            _logger = logger;
        }

        public async Task<ReservationDto> CreateReservationAsync(int userId, CreateReservationDto dto)
        {
            // Check if car has active reservations
            var hasActiveReservation = await _context.Reservations
            .AnyAsync(r => r.CarId == dto.CarId
                       && r.Status == SessionStatus.Active);

            if (hasActiveReservation)
            {
                throw new InvalidOperationException(
                    "This car already has an active parking reservation");
            }
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
                CreatedAt = DateTime.UtcNow,
                Status = SessionStatus.Reserved,  // Initial status is Reserved

            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            reservation.QRCode = _qrCodeService.GenerateQRCode(
                     reservation.Id,
                     userId,
                     DateTime.UtcNow
                 );
          
            // Update spot status
            spot.Status = SpotStatus.Reserved;
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

            if (reservation.Status != SessionStatus.Reserved)
                throw new InvalidOperationException("Reservation is not in a valid state to start parking");

            if (reservation.EntryTime.HasValue)
                throw new InvalidOperationException("Parking has already started for this reservation");


            reservation.EntryTime = DateTime.UtcNow;
            reservation.Status = SessionStatus.Active;
            reservation.ParkingSpot.Status = SpotStatus.Occupied;
            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto> EndParkingAsync(string qrCode)
        {
            var reservation = await _context.Reservations
                       .Include(r => r.ParkingSpot)
                           .ThenInclude(ps => ps.ParkingZone)
                       .FirstOrDefaultAsync(r => r.QRCode == qrCode);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            if (reservation.ParkingSpot == null)
                throw new InvalidOperationException("Parking spot not found for this reservation");

            if (reservation.ParkingSpot.ParkingZone == null)
                throw new InvalidOperationException("Parking zone not found for this spot");

            if (reservation.ExitTime == null)
            {
                reservation.ExitTime = DateTime.UtcNow;
            }

            try
            {
                // Calculate fee
                if(reservation.Status != SessionStatus.Active)
                    throw new InvalidOperationException("Reservation is not in a valid state to end parking");
                
                reservation.TotalAmount = await _parkingZoneService.CalculateParkingFee(
                    reservation.ParkingSpot.ParkingZone.Id,
                    reservation.EntryTime.Value,
                    reservation.ExitTime.Value
                );

                _logger.LogInformation(
                    "Calculated parking fee for reservation {ReservationId}: {Amount}",
                    reservation.Id,
                    reservation.TotalAmount);

                reservation.Status = SessionStatus.Completed;

                // Update parking spot status
                reservation.ParkingSpot.Status = SpotStatus.Available;
                reservation.ParkingSpot.ReservationId = null;

                await _context.SaveChangesAsync();

                return _mapper.Map<ReservationDto>(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calculating parking fee for reservation {ReservationId}",
                    reservation.Id);
                throw;
            }
        }
        public async Task<ReservationDto> CancelReservationAsync(int reservationId, int userId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.ParkingSpot)
                    .ThenInclude(ps => ps.ParkingZone)
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            // Check if reservation can be cancelled
            if (reservation.Status != SessionStatus.Reserved)
                throw new InvalidOperationException("Only reserved status reservations can be cancelled");

            try
            {
                // Calculate any applicable cancellation fee
                var cancellationFee = CalculateCancellationFee(reservation);

                // Update reservation status
                reservation.Status = SessionStatus.Cancelled;
                reservation.TotalAmount = cancellationFee;

                // Free up the parking spot
                if (reservation.ParkingSpot != null)
                {
                    reservation.ParkingSpot.Status = SpotStatus.Available;
                    reservation.ParkingSpot.ReservationId = null;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Reservation {ReservationId} cancelled. Cancellation fee: {Fee}",
                    reservationId,
                    cancellationFee);

                return _mapper.Map<ReservationDto>(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error cancelling reservation {ReservationId}",
                    reservationId);
                throw;
            }
        }



        private async Task<ReservationDto> GetReservationDtoAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            return _mapper.Map<ReservationDto>(reservation);
        }


        private decimal CalculateCancellationFee(Reservation reservation)
        {
            // - If cancelled within 15 minutes of creation: no fee
            // - If cancelled after 15 minutes: 1 hour parking fee

            var timeSinceCreation = DateTime.UtcNow - reservation.CreatedAt;
            if (timeSinceCreation.TotalMinutes <= 15)
                return 0;

            // Charge one hour of parking
            return reservation.ParkingSpot.ParkingZone.HourlyRate;
        }
        public async Task<decimal> CalculateParkingFeeAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .ThenInclude(ps => ps.ParkingZone)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            if (reservation.Status != SessionStatus.Active)
                throw new InvalidOperationException("Reservation is not in a valid state to calculate fee");


            var exitTime = reservation.ExitTime ?? DateTime.UtcNow;

            return await _parkingZoneService.CalculateParkingFee(
                reservation.ParkingSpot.ParkingZone.Id,
                reservation.EntryTime.Value,
                exitTime);
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

        public async Task<ReservationDto> GetActiveReservation(int carId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.ParkingSpot)
                    .ThenInclude(ps => ps.ParkingZone)
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.CarId == carId
                                     && r.Status == SessionStatus.Active);

            if (reservation == null)
                throw new KeyNotFoundException("No active reservation found for this car");

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<bool> HasActiveReservation(int carId)
        {
            var hasActive = await _context.Reservations
                .AnyAsync(r => r.CarId == carId && (r.Status == SessionStatus.Active||r.Status == SessionStatus.Reserved));

            if (hasActive)
            {
                _logger.LogInformation("Car {CarId} has an active reservation", carId);
            }

            return hasActive;
        }

    }
}
