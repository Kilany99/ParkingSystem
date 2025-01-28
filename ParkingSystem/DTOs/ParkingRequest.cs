using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.DTOs
{
    public class ParkingRequest
    {
        [Required]
        [RegularExpression(@"^[A-Z]{3}\d{4}$", ErrorMessage = "Invalid plate format")]   //validation that plateNumber is in the correct form
        public string PlateNumber { get; set; } // From ALPR camera

        [Required]
        public string QrCode { get; set; } // From QR scanner
    }
}
