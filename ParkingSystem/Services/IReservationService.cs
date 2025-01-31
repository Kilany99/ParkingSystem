using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.DTOs;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using System.Text.RegularExpressions;
using static ParkingSystem.DTOs.PaymentDtos;
using static ParkingSystem.DTOs.ReservationDtos;

namespace ParkingSystem.Services
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(int userId, CreateReservationDto dto);
        Task<ReservationDto> StartParkingAsync(ParkingRequest request);
        Task<ReservationDto> EndParkingAsync(ParkingRequest request, PaymentMethod method);
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
        private readonly IPaymentService _paymentService;
        private readonly Regex _plateRegex = new(@"^[A-Z]{3}\d{4}$", RegexOptions.Compiled);

        public ReservationService(AppDbContext context, IMapper mapper, IQRCodeService qrCodeService,
            IParkingZoneService parkingZoneService, ILogger<ReservationService> logger, IPaymentService paymentService)
        {
            _context = context;
            _mapper = mapper;
            _qrCodeService = qrCodeService;
            _parkingZoneService = parkingZoneService;
            _logger = logger;
            _paymentService = paymentService;
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
                .FirstOrDefaultAsync(c => c.Id == dto.CarId && c.UserId == userId)??
                    throw new InvalidOperationException("Car not found or doesn't belong to user");

            //verfy that parking zone not full
            var parkingZone = await _context.ParkingZones.FindAsync(dto.ParkingZoneId)?? 
                throw new InvalidOperationException("Parking zone not found");
           if (parkingZone.IsFull)
                throw new InvalidOperationException($"Parking zone {parkingZone.Name} is full");


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

        public async Task<ReservationDto> StartParkingAsync(ParkingRequest request)
        {
            //validate qr code
            if (!_qrCodeService.ValidateQRCode(request.QrCode))
                throw new InvalidOperationException("Invalid QR code");
            // Validate plate number format 
            if (!_plateRegex.IsMatch(request.PlateNumber))
                throw new InvalidOperationException("Invalid license plate format");

            var (reservationId, userId, timeStamp) = _qrCodeService.DecodeQRCode(request.QrCode);
        
            // Check timestamp freshness (24 hour window) 
            if (DateTime.UtcNow - timeStamp > TimeSpan.FromHours(24))
                throw new InvalidOperationException("Expired QR code");

            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId) ?? throw new KeyNotFoundException("Reservation not found");
            //validate reservation state
            if (reservation.Status != SessionStatus.Reserved)
                throw new InvalidOperationException("Reservation is not in a valid state to start parking");
            //validate entryTime
            if (reservation.EntryTime.HasValue)
                throw new InvalidOperationException("Parking has already started for this reservation");
            // Validate plate match
            if (!reservation.Car.PlateNumber.Equals(request.PlateNumber, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Scanned plate number does not match reservation");


            reservation.EntryTime = DateTime.UtcNow;
            reservation.Status = SessionStatus.Active;
            reservation.ParkingSpot.Status = SpotStatus.Occupied;
            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto> EndParkingAsync(ParkingRequest request,PaymentMethod method)
        {
            if (!Enum.IsDefined(typeof(PaymentMethod), method))
            {
                throw new ArgumentException("Invalid payment method selected");
            }

            // Validate plate number format 
            if (!_plateRegex.IsMatch(request.PlateNumber)||string.IsNullOrEmpty(request.PlateNumber))
                throw new InvalidOperationException("plate format in not correct");
            //validate qr code
            if (!_qrCodeService.ValidateQRCode(request.QrCode)||string.IsNullOrEmpty(request.QrCode))
                throw new InvalidOperationException("Invalid QR code");
            var reservation = await _context.Reservations
                        .Include(r => r.Car)
                       .Include(r => r.ParkingSpot)
                            .ThenInclude(ps => ps.ParkingZone)
                       .FirstOrDefaultAsync(r => r.QRCode == request.QrCode);
            if(reservation ==null)
                throw new KeyNotFoundException("Reservation not found");
            //validate plate number
            if (string.IsNullOrEmpty(reservation.Car.PlateNumber)||
                !reservation.Car.PlateNumber.Equals(request.PlateNumber, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Scanned plate number does not match reservation");
            if (reservation.Status != SessionStatus.Active)
                throw new InvalidOperationException("Reservation is not in a valid state to end parking");

            if (reservation.EntryTime == null)
                throw new InvalidOperationException("Cannot end parking for not started reservation!");
            
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
                
                reservation.TotalAmount = await _parkingZoneService.CalculateParkingFee(
                    reservation.ParkingSpot.ParkingZone.Id,
                    reservation.EntryTime.Value,
                    reservation.ExitTime.Value
                );

                _logger.LogInformation(
                    "Calculated parking fee for reservation {ReservationId}: {Amount}",
                    reservation.Id,
                    reservation.TotalAmount);
                //complete payment first in case of online payemnt while making reservation
                ProcessPaymentDto paymentDto = new (reservation.Id, reservation.TotalAmount.Value, method);
                await _paymentService.ProcessPaymentAsync(reservation.Id, paymentDto);
                reservation.IsPaid = true;
                reservation.Status = SessionStatus.Completed;

                // Update parking spot status
                reservation.ParkingSpot.Status = SpotStatus.Available;
                reservation.ParkingSpot.ReservationId = null;

                await _context.SaveChangesAsync();

                return _mapper.Map<ReservationDto>(reservation);
            }
            catch(Exception ex ) when (ex.Message.Contains($"Payment:"))
            {
                throw new InvalidOperationException("There was an issue during processing payment" + ex.Message);
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
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId)??
                    throw new KeyNotFoundException("Reservation not found");

            if (reservation.Car == null)
                throw new InvalidOperationException("No car associated with this reservation");
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
                    $"Error cancelling reservation {reservation.Id}",
                    reservationId);
                throw;
            }
        }

        public async Task<decimal> CalculateParkingFeeAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .ThenInclude(ps => ps.ParkingZone)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found");

            if (reservation.Status != SessionStatus.Active || reservation.EntryTime == null)
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

        public async Task<bool> HasActiveReservation(int carId) =>
             await _context.Reservations
                .AnyAsync(r => r.CarId == carId && (r.Status == SessionStatus.Active||r.Status == SessionStatus.Reserved));

        private async Task<ReservationDto> GetReservationDtoAsync(int reservationId) =>
         _mapper.Map<ReservationDto>(await _context.Reservations
             .Include(r => r.Car)
             .Include(r => r.ParkingSpot)
             .FirstOrDefaultAsync(r => r.Id == reservationId)
             );



        private decimal CalculateCancellationFee(Reservation reservation)
        {
            // - If cancelled within 15 minutes of creation: no fee
            // - If cancelled after 15 minutes: 1/5 per hour of parking fee for online paid reservations
            if (reservation.ParkingSpot == null || reservation.ParkingSpot.ParkingZone == null)
                throw new InvalidOperationException("Cannot get fee for this reservation");
            if (reservation.IsPaid)
            {
                if (WithinCnxDuration(reservation))
                    return 0;

                // Charge 1/5 of parking fee
                var duration = (DateTime.UtcNow - reservation.CreatedAt).Hours;
                return (decimal)0.2 * reservation.ParkingSpot.ParkingZone.HourlyRate * duration;
            }
            else
                return 0; //non paid reservation with no charges
        }
        private bool WithinCnxDuration(Reservation reservation)
        {
            var timeSinceCreation = DateTime.UtcNow - reservation.CreatedAt;
            if (timeSinceCreation.TotalMinutes <= 15)
                return true;
            return false;

        }



    }
}
