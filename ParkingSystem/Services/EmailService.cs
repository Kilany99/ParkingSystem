using System.Net;
using System.Net.Mail;

namespace ParkingSystem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendPasswordResetEmail(string email, string resetToken)
        {
            var resetLink = $"{_configuration["AppSettings:ClientUrl"]}/reset-password?token={resetToken}";

            // Use your preferred email sending service (SMTP, SendGrid, etc.)
            var smtpClient = new SmtpClient("smtp.mailtrap.io");
            var message = new MailMessage
            {
                From = new MailAddress("no-reply@example.com"),
                Subject = "Password Reset",
                Body = $"Click here to reset your password: {resetLink}",
                IsBodyHtml = true,
            };
            message.To.Add(email);

            await smtpClient.SendMailAsync(message);
        }
    }
}
