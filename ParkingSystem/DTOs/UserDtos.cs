namespace ParkingSystem.DTOs
{
    public class UserDtos
    {
        public record UserDto(
            int Id,
            string Email,
            string Name,
            string Phone,
            string Role);


        public record UpdateUserDto(
            string Name,
            string Phone);

        public record CreateUserDto(
        
            string Email,
            string Name,
            string Phone,
            string Role,
            string Password 
        );
    }

}
