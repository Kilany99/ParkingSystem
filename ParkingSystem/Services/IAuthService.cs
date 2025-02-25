﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParkingSystem.Data;
using ParkingSystem.Models;
using ParkingSystem.Settings;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ParkingSystem.DTOs.AuthDtos;

namespace ParkingSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<ActionResult> ForgotPasswordAsync(ForgotPasswordDto model);
        Task<ActionResult> ResetPasswordAsync(ResetPasswordDto model);
    }
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;


        public AuthService(
            AppDbContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());


            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            return new AuthResponseDto(token, user.Email, user.Name, user.Role);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                throw new InvalidOperationException("Email already registered");
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Name = model.Name,
                Phone = model.Phone,
                Role = "Admin" // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return new AuthResponseDto(token, user.Email, user.Name, user.Role);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"]));

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<ActionResult> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return new BadRequestObjectResult("Email not found.");
            }

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiration = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Send the token to the user's email 
            await _emailService.SendPasswordResetEmail(user.Email, resetToken);

            return new OkObjectResult("Password reset link has been sent to your email.");
        }

        public async Task<ActionResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetPasswordToken == model.Token);

            if (user == null || user.ResetPasswordTokenExpiration < DateTime.UtcNow)
            {
                return new BadRequestObjectResult("Invalid or expired token.");
            }

            // Hash the new password
            user.PasswordHash = HashPassword(model.NewPassword);
            user.ResetPasswordToken = null; // Clear the token after successful reset
            user.ResetPasswordTokenExpiration = null; // Clear the expiration

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Password has been reset successfully.");
        }

        private string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);
        

        private bool VerifyPassword(string password, string hash)=>
             BCrypt.Net.BCrypt.Verify(password, hash);
    }

}
