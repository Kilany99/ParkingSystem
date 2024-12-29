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
    }
    public class ParkingZoneService : IParkingZoneService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ParkingZoneService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ParkingZoneDto> CreateZoneAsync(CreateParkingZoneDto dto)
        {
            var zone = new ParkingZone
            {
                Name = dto.Name,
                TotalFloors = dto.TotalFloors,
                SpotsPerFloor = dto.SpotsPerFloor,
                Description = dto.Description
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
    }

}
