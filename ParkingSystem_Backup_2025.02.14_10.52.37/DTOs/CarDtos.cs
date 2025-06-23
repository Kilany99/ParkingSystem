using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.DTOs
{
    public class CarDtos
    {
        public record CreateCarDto(
            [RegularExpression(@"^[A-Z]{3}\d{4}$", ErrorMessage = "Invalid plate format")]   //validation that plateNumber is in the correct form "XXX1234"
            string PlateNumber,
            string Model,
            string Color);
        public record UpdateCarDto(string Model, string Color);
        public record CarDto(int Id,
            [RegularExpression(@"^[A-Z]{3}\d{4}$", ErrorMessage = "Invalid plate format")]   //validation that plateNumber is in the correct form "XXX1234"
            string PlateNumber, 
            string Model, 
            string Color);
    }
}
