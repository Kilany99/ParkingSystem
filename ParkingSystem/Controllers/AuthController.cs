using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Attributes;
using ParkingSystem.Data;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.AuthDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
       
        public AuthController(IAuthService authService, AppDbContext appDbContext,EmailService emailService) 
        {
            _authService = authService;
            
        }
        [HttpPost("register")]
        [CustomRateLimit("1h", 3)]  // 3 registrations per hour
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto model)
        {
            try
            {
                var result = await _authService.RegisterAsync(model);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [CustomRateLimit("5m", 5)]  // 5 attempts per 5 minutes
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
        {
            Console.WriteLine("I am here 1");

            try
            {
                var result = await _authService.LoginAsync(model);
                Console.WriteLine("I am here 2");
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("I am here 3");

                return Unauthorized("Invalid email or password");
            }
        }
        [HttpPost("forgot-password")]
        [CustomRateLimit("1h", 1)]  // 1 attempt per 1 hour
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            try
            {
                await _authService.ForgotPasswordAsync(model);
                return Ok("Password reset link has been sent.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        [CustomRateLimit("1h", 1)]  // 1 attempt per 1 hour
        public async Task<ActionResult> ResetPassword(ResetPasswordDto model)
        {
            try
            {
                await _authService.ResetPasswordAsync(model);
                return Ok("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
