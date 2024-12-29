namespace ParkingSystem.DTOs
{
    public class CarDtos
    {
        public record CreateCarDto(string PlateNumber, string Model, string Color);
        public record UpdateCarDto(string Model, string Color);
        public record CarDto(int Id, string PlateNumber, string Model, string Color);
    }
}
