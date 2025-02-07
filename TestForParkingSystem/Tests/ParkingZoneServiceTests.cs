using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace TestForParkingSystem.Tests
{
    public class ParkingZoneServiceTests : TestBase
    {
        private readonly ParkingZoneService _service;

        public ParkingZoneServiceTests()
        {
            _service = new ParkingZoneService(
                _context,
                _mapper,
                Mock.Of<ILogger<ParkingZoneService>>());
        }

        [Fact]
        public async Task CreateZone_WithValidData_ShouldSucceed()
        {
            // Arrange
            var dto = new CreateParkingZoneDto(
                "Test Zone",
                2,
                10,
                "Test Description",
                10.0m);

            // Act
            var result = await _service.CreateZoneAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.HourlyRate, result.HourlyRate);

            var spots = await _context.ParkingSpots
                .Where(s => s.ParkingZone.Name == dto.Name)
                .ToListAsync();
            Assert.Equal(dto.TotalFloors * dto.SpotsPerFloor, spots.Count);
        }

        [Fact]
        public async Task IsZoneFull_WhenAllSpotsOccupied_ShouldReturnTrue()
        {
            // Arrange
            var zone = new ParkingZone
            {
                Id = 1,
                Name = "Test Zone"
            };

            var spot = new ParkingSpot
            {
                ParkingZoneId = zone.Id,
                Status = SpotStatus.Occupied,
                ParkingZone = zone,
            };

            _context.ParkingZones.Add(zone);
            _context.ParkingSpots.Add(spot);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.IsZoneFull(zone.Id);

            // Assert
            Assert.True(result);
        }
    }
}
