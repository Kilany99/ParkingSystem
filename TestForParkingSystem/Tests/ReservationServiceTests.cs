using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.ReservationDtos;

namespace TestForParkingSystem.Tests
{
    public class ReservationServiceTests : TestBase
    {
        private readonly ReservationService _service;
        private readonly Mock<IParkingZoneService> _parkingZoneServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IQRCodeService> _qrCodeServiceMock;

        public ReservationServiceTests()
        {
            _parkingZoneServiceMock = new Mock<IParkingZoneService>();
            _mapperMock = new Mock<IMapper>();
            _qrCodeServiceMock = new Mock<IQRCodeService>();
            _service = new ReservationService(
                _context,
                _mapperMock.Object,
                _qrCodeServiceMock.Object,
                _parkingZoneServiceMock.Object,
                _loggerMock.Object
                );
        }

        [Fact]
        public async Task CreateReservation_WithValidData_ShouldSucceed()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "test@test.com",
                Name = "Test User"
            };

            var car = new Car
            {
                Id = 1,
                UserId = userId,
                PlateNumber = "ABC123"
            };

            var parkingZone = new ParkingZone
            {
                Id = 1,
                Name = "Test Zone",
                HourlyRate = 10
            };

            var parkingSpot = new ParkingSpot
            {
                Id = 1,
                ParkingZoneId = parkingZone.Id,
                Status = SpotStatus.Available
            };

            _context.Users.Add(user);
            _context.Cars.Add(car);
            _context.ParkingZones.Add(parkingZone);
            _context.ParkingSpots.Add(parkingSpot);
            await _context.SaveChangesAsync();

            var dto = new CreateReservationDto(car.Id, parkingSpot.Id);

            // Act
            var result = await _service.CreateReservationAsync(userId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(SessionStatus.Reserved, result.Status);
            Assert.Null(result.EntryTime);

            var spot = await _context.ParkingSpots.FindAsync(parkingSpot.Id);
            Assert.Equal(SpotStatus.Reserved, spot.Status);
        }

        [Fact]
        public async Task CreateReservation_WithActiveReservation_ShouldThrowException()
        {
            // Arrange
            var userId = 1;
            var carId = 1;
            var existingReservation = new Reservation
            {
                UserId = userId,
                CarId = carId,
                Status = SessionStatus.Active
            };

            _context.Reservations.Add(existingReservation);
            await _context.SaveChangesAsync();

            var dto = new CreateReservationDto(carId, 1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateReservationAsync(userId, dto));
        }

        [Fact]
        public async Task StartParking_WithValidReservation_ShouldSucceed()
        {
            // Arrange
            var reservation = new Reservation
            {
                Id = 1,
                UserId = 1,
                CarId = 1,
                ParkingSpotId = 1,
                Status = SessionStatus.Reserved,
                QRCode = "test-qr"
            };

            var parkingSpot = new ParkingSpot
            {
                Id = 1,
                Status = SpotStatus.Reserved
            };

            _context.ParkingSpots.Add(parkingSpot);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.StartParkingAsync("test-qr");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(SessionStatus.Active, result.Status);
            Assert.NotNull(result.EntryTime);

            var spot = await _context.ParkingSpots.FindAsync(parkingSpot.Id);
            Assert.Equal(SpotStatus.Occupied, spot.Status);
        }

        [Fact]
        public async Task CancelReservation_WithinGracePeriod_ShouldHaveNoFee()
        {
            // Arrange
            var userId = 1;
            var reservation = new Reservation
            {
                Id = 1,
                UserId = userId,
                Status = SessionStatus.Reserved,
                CreatedAt = DateTime.UtcNow,
                ParkingSpotId = 1
            };

            var parkingSpot = new ParkingSpot
            {
                Id = 1,
                Status = SpotStatus.Reserved,
                ParkingZone = new ParkingZone { HourlyRate = 10 }
            };

            _context.ParkingSpots.Add(parkingSpot);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CancelReservationAsync(1, userId);

            // Assert
            Assert.Equal(SessionStatus.Cancelled, result.Status);
            Assert.Equal(0, result.TotalAmount);

            var spot = await _context.ParkingSpots.FindAsync(parkingSpot.Id);
            Assert.Equal(SpotStatus.Available, spot.Status);
        }
    }
}
