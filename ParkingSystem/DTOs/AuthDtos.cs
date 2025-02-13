namespace ParkingSystem.DTOs
{
    public class AuthDtos
    {
        public record LoginDto(string Email, string Password);

        public record RegisterDto(
            string Email,
            string Password,
            string Name,
            string Phone);

        public record AuthResponseDto(
            string Token,
            string Email,
            string Name,
            string Role);    

        public record ForgotPasswordDto
            (
            string Email
        );

        public record ResetPasswordDto
            (
            string NewPassword,
            string Token
        );
    }


}
