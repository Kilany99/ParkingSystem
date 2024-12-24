using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.DTOs
{
    public class UpdateReservaion
    {
        public int ReservaionId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

    }
}
