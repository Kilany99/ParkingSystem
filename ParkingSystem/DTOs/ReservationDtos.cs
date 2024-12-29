using ParkingSystem.Enums;
using static ParkingSystem.DTOs.CarDtos;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace ParkingSystem.DTOs
{
    public class ReservationDtos
    {
        public record CreateReservationDto(int CarId, int ParkingSpotId);
        public record ReservationDto(
            int Id,
            DateTime EntryTime,
            DateTime? ExitTime,
            decimal? TotalAmount,
            string QRCode,
            SessionStatus Status,
            CarDto Car,
            ParkingSpotDto ParkingSpot
        );
    
    }
}
