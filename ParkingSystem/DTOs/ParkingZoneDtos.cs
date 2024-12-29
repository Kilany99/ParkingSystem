using ParkingSystem.Enums;

namespace ParkingSystem.DTOs
{
    public class ParkingZoneDtos
    {
        public record CreateParkingZoneDto(string Name, int TotalFloors, int SpotsPerFloor, string? Description);
        public record ParkingZoneDto(int Id, string Name, int TotalFloors, int SpotsPerFloor, bool IsFull);
        public record ParkingZoneStatusDto
        {
            public int ZoneId { get; init; }
            public string ZoneName { get; init; } = string.Empty;
            public int TotalSpots { get; init; }
            public int AvailableSpots { get; init; }
            public bool IsFull { get; init; }
        }
        public record ParkingSpotDto(int Id, string SpotNumber, int Floor, SpotStatus Status, SpotType Type);

    }
}
