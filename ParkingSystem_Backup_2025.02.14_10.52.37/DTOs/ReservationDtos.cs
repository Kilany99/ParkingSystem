using ParkingSystem.Enums;
using System.Text.Json.Serialization;
using static ParkingSystem.DTOs.CarDtos;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace ParkingSystem.DTOs
{
    public class ReservationDtos
    {
        public record CreateReservationDto(int CarId, int ParkingSpotId,int ParkingZoneId);
        public class ReservationDto
        {
            public int Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? EntryTime { get; set; }
            public DateTime? ExitTime { get; set; }
            public decimal? TotalAmount { get; set; }
            public string QRCode { get; set; } = string.Empty;
            public bool IsPaid { get; set; }
            public SessionStatus Status { get; set; }
            public CarDto? Car { get; set; }
            public ParkingSpotDto ParkingSpot { get; set; }
            public ParkingZoneDto? ParkingZone { get; set; }

            public ReservationDto() { }
        }

    }
}
