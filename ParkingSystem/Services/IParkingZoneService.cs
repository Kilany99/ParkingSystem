using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.CarDtos;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace ParkingSystem.Services
{
    public interface IParkingZoneService
    {
        Task<ParkingZoneDto> CreateZoneAsync(CreateParkingZoneDto dto);
        Task<IEnumerable<ParkingZoneDto>> GetAllZonesAsync();
        Task<ParkingZoneStatusDto> GetZoneStatusAsync(int zoneId);
        Task<IEnumerable<ParkingSpotDto>> GetSpotsAsync(int zoneId, SpotStatus status);
        Task<IEnumerable<ParkingSpotDto>> GetAllSpotsAsync(int zoneId);
        Task<IEnumerable<Car>> GetCarsInZoneAsync(int zoneId);
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
                        Status = SpotStatus.Available,
                        ParkingZone = zone
                    };

                    _context.ParkingSpots.Add(parkingSpot);
                }
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ParkingZoneDto>(zone);
        }

        public async Task<IEnumerable<ParkingZoneDto>> GetAllZonesAsync() =>
            _mapper.Map<IEnumerable<ParkingZoneDto>>(await _context.ParkingZones
                .Include(z => z.ParkingSpots)
                .ToListAsync());


            public async Task<ParkingZoneStatusDto> GetZoneStatusAsync(int zoneId)
            {
                var zone = await _context.ParkingZones
                    .Include(z => z.ParkingSpots)
                    .FirstOrDefaultAsync(z => z.Id == zoneId)??
                    throw new KeyNotFoundException("Parking zone not found");

                var spots = zone.ParkingSpots;
                // Calculate distribution
                var distribution = new ParkingSpotDistributionDto
                {
                    Available = spots.Count(s => s.Status == SpotStatus.Available),
                    Occupied = spots.Count(s => s.Status == SpotStatus.Occupied),
                    Reserved = spots.Count(s => s.Status == SpotStatus.Reserved),
                    Maintenance = spots.Count(s => s.Status == SpotStatus.Maintenance),
                    AvailableByFloor = spots
                       .Where(s => s.Status == SpotStatus.Available)
                       .GroupBy(s => s.Floor)
                       .ToDictionary(
                           g => g.Key,
                           g => g.Count()
                       )
                };
                return new ParkingZoneStatusDto
                {
                    ZoneId = zone.Id,
                    ZoneName = zone.Name,
                    TotalSpots = spots.Count,
                    AvailableSpots = distribution.Available,
                    IsFull = distribution.Available == 0,
                    Distribution = distribution
                };
            }

        public async Task<IEnumerable<ParkingSpotDto>> GetSpotsAsync(int zoneId, SpotStatus status)
        {
            // Validate status value
            if (!Enum.IsDefined(typeof(SpotStatus), status))
            {
                throw new ArgumentException("Invalid spot status");
            }

            // Fetch ParkingSpots along with their CurrentReservation details
            var spotsWithReservations = await _context.ParkingSpots
                .Where(s => s.ParkingZoneId == zoneId && s.Status == status)
                .Include(s => s.CurrentReservation) // Eager load the Reservation related to the Spot
                .ThenInclude(r => r.Car) // Optional: Eager load the Car data 
                .Include(s => s.ParkingZone) // Eager load ParkingZone 
                .ToListAsync();

            // Map the fetched data to ParkingSpotDto, including the reservation details
            return _mapper.Map<IEnumerable<ParkingSpotDto>>(spotsWithReservations);
        }

        public async Task<IEnumerable<Car>> GetCarsInZoneAsync(int zoneId)
        {
            var cars = await _context.Cars
                .Where(c => c.ParkingZoneId == zoneId && c.Reservations.Any(r => r.Status == SessionStatus.Active))
                .Include(c => c.Reservations.Where(r => r.Status == SessionStatus.Active))
                .ToListAsync();

            return cars;
        }


        public async Task<bool> IsZoneFull(int zoneId)
        {
            var zone = await _context.ParkingZones
                .Include(z => z.ParkingSpots)
                .FirstOrDefaultAsync(z => z.Id == zoneId)??
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
        public async Task<IEnumerable<ParkingSpotDto>> GetAllSpotsAsync(int zoneId)
        {
            var spots = await _context.ParkingSpots
                .Where(s => s.ParkingZoneId == zoneId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ParkingSpotDto>>(spots);
        }

        private decimal GetHourlyRate(ParkingZone zone, DateTime time)=>
            // Standard rate
             zone.HourlyRate;
    }

}
