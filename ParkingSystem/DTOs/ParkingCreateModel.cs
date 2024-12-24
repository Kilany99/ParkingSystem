using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.DTOs
{
    public class ParkingCreateModel
    {
        [Required]
        public string ParkingName { get; set; }

        [Required]
        public string Parkingfee { get; set; }

        [Required]
        public int ParkingCapacity { get; set; }
    }
}
