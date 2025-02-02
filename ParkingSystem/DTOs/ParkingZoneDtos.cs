using ParkingSystem.Enums;
using ParkingSystem.Models;
using System.Text.Json.Serialization;
using static ParkingSystem.DTOs.CarDtos;
using static ParkingSystem.DTOs.ReservationDtos;

namespace ParkingSystem.DTOs
{
    public class ParkingZoneDtos
    {
        public record CreateParkingZoneDto(string Name, int TotalFloors,
            int SpotsPerFloor, string? Description
            ,decimal HourlyRate);
        public record ParkingZoneDto(int Id, string Name, int TotalFloors, int SpotsPerFloor, bool IsFull, decimal HourlyRate);
        public record ParkingZoneStatusDto
        {
            public int ZoneId { get; init; }
            public string ZoneName { get; init; } = string.Empty;
            public int TotalSpots { get; init; }
            public int AvailableSpots { get; init; }
            public decimal HourlyRate { get; init; }
            public bool IsFull { get; init; }
            public required ParkingSpotDistributionDto Distribution { get; init; }

        }
        public record ParkingSpotDistributionDto
        {
            public int Available { get; init; }
            public int Occupied { get; init; }
            public int Reserved { get; init; }
            public int Maintenance { get; init; }
            public required Dictionary<int, int> AvailableByFloor { get; init; }
        }
        public record ParkingSpotDto(int Id,
             string SpotNumber ,
             int Floor ,
             SpotStatus Status,
             SpotType Type,
             ReservationDto? CurrentReservation,
             ParkingZoneDto ParkingZone);

    }
}
