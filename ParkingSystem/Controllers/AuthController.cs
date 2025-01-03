﻿using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Attributes;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.AuthDtos;

namespace ParkingSystem.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
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
            try
            {
                var result = await _authService.LoginAsync(model);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid email or password");
            }
        }

    }
}
