using Microsoft.AspNetCore.Mvc;
using ParkingSystem.DTOs.Events;
using ParkingSystem.Services;

namespace ParkingSystem.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly EmailService _emailService;

        public NotificationController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] EmailNotificationRequest dto)
        {
            string subject = "Parking Reservation Notification";
            string htmlBody = $"<h1>Notification</h1><p>{dto.Message}</p>";

            // Optionally, generate or attach a QR code image as a byte array.

            await _emailService.SendNotificationEmailAsync(dto.Email, subject, htmlBody, dto.QrCode);
            return Ok(new { Message = "Notification email sent successfully!" });
        }
    }
}
