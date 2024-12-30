using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace ParkingSystem.Services
{
    public interface IParkingZoneService
    {
        Task<ParkingZoneDto> CreateZoneAsync(CreateParkingZoneDto dto);
        Task<IEnumerable<ParkingZoneDto>> GetAllZonesAsync();
        Task<ParkingZoneStatusDto> GetZoneStatusAsync(int zoneId);
        Task<IEnumerable<ParkingSpotDto>> GetAvailableSpotsAsync(int zoneId);

        Task<decimal> CalculateParkingFee(int zoneId, DateTime entryTime, DateTime exitTime);
        Task<bool> IsZoneFull(int zoneId);
    }
    public class ParkingZoneService : IParkingZoneService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ParkingZoneService> _logger; 

        public ParkingZoneService(AppDbContext context, IMapper mapper, ILogger<ParkingZoneService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ParkingZoneDto> CreateZoneAsync(CreateParkingZoneDto dto)
        {
            var zone = new ParkingZone
            {
                Name = dto.Name,
                TotalFloors = dto.TotalFloors,
                SpotsPerFloor = dto.SpotsPerFloor,
                Description = dto.Description,
                HourlyRate = dto.HourlyRate
            };

            _context.ParkingZones.Add(zone);
            await _context.SaveChangesAsync();

            // Create parking spots for this zone
            for (int floor = 1; floor <= dto.TotalFloors; floor++)
            {
                for (int spot = 1; spot <= dto.SpotsPerFloor; spot++)
                {
                    var parkingSpot = new ParkingSpot
                    {
                        ParkingZoneId = zone.Id,
                        Floor = floor,
                        SpotNumber = $"F{floor}S{spot}",
                        Status = SpotStatus.Available
                    };

                    _context.ParkingSpots.Add(parkingSpot);
                }
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ParkingZoneDto>(zone);
        }

        public async Task<IEnumerable<ParkingZoneDto>> GetAllZonesAsync()
        {
            var zones = await _context.ParkingZones
                .Include(z => z.ParkingSpots)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ParkingZoneDto>>(zones);
        }

        public async Task<ParkingZoneStatusDto> GetZoneStatusAsync(int zoneId)
        {
            var zone = await _context.ParkingZones
                .Include(z => z.ParkingSpots)
                .FirstOrDefaultAsync(z => z.Id == zoneId);

            if (zone == null)
                throw new KeyNotFoundException("Parking zone not found");

            var availableSpots = zone.ParkingSpots.Count(s => s.Status == SpotStatus.Available);
            var totalSpots = zone.ParkingSpots.Count;

            return new ParkingZoneStatusDto
            {
                ZoneId = zone.Id,
                ZoneName = zone.Name,
                TotalSpots = totalSpots,
                AvailableSpots = availableSpots,
                IsFull = availableSpots == 0
            };
        }

        public async Task<IEnumerable<ParkingSpotDto>> GetAvailableSpotsAsync(int zoneId)
        {
            var spots = await _context.ParkingSpots
                .Where(s => s.ParkingZoneId == zoneId && s.Status == SpotStatus.Available)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ParkingSpotDto>>(spots);
        }

     
        public async Task<bool> IsZoneFull(int zoneId)
        {
            var zone = await _context.ParkingZones
                .Include(z => z.ParkingSpots)
                .FirstOrDefaultAsync(z => z.Id == zoneId);

            if (zone == null)
                throw new KeyNotFoundException($"Parking zone {zoneId} not found");

            var availableSpots = zone.ParkingSpots.Count(s => s.Status == SpotStatus.Available);
            var isFull = availableSpots == 0;

            // Update IsFull status if it's changed
            if (zone.IsFull != isFull)
            {
                zone.IsFull = isFull;
                await _context.SaveChangesAsync();
            }

            return isFull;
        }

        public async Task<decimal> CalculateParkingFee(int zoneId, DateTime entryTime, DateTime exitTime)
        {
            var zone = await _context.ParkingZones
                .FirstOrDefaultAsync(z => z.Id == zoneId);

            if (zone == null)
                throw new KeyNotFoundException($"Parking zone {zoneId} not found");

            var duration = exitTime - entryTime;
            var hours = Math.Ceiling(duration.TotalHours); // Round up to the nearest hour

            var totalFee = (decimal)hours * zone.HourlyRate;

            _logger.LogInformation(
                "Calculated parking fee for zone {ZoneId}: {Hours} hours * {Rate} = {Total}",
                zoneId, hours, zone.HourlyRate, totalFee);

            return totalFee;
        }

        private decimal GetHourlyRate(ParkingZone zone, DateTime time)
        {
            // Standard rate
            return zone.HourlyRate;
        }
    }

}
