using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ParkingSystem.DTOs;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using ParkingSystem.Helpers;

namespace ParkingSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendPasswordResetEmail(string email, string resetToken);
        Task SendNotificationEmailAsync(string recipientEmail, string subject, string htmlBody, string qrCode);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailSettings _emailSettings;
        private readonly QRGenerationHelper _qrGenerationHelper;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings,IConfiguration configuration, QRGenerationHelper qrGenerationHelper, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _emailSettings = emailSettings.Value;
            _qrGenerationHelper = qrGenerationHelper;
            _logger = logger;
        }
        public async Task SendNotificationEmailAsync(string recipientEmail, string subject,string htmlBody, string qrCode)
        {
            // Create the email message.
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Parking System", _emailSettings.GmailUser));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;

            // Build the email body.
            var builder = new BodyBuilder { HtmlBody = htmlBody };

            if (qrCode != null)
            {
                try
                {
                    byte[] qrImageBytes = _qrGenerationHelper.GenerateQRCodeImageBytes(qrCode);
                    // "QRCode.png" is the attachment filename.
                    builder.Attachments.Add("QRCode.png", qrImageBytes, new ContentType("image", "png"));
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("An error occured during generating QR image" + ex.Message);
                    throw;
                }
            }
            message.Body = builder.ToMessageBody();

            // Send the email using MailKit's SmtpClient.
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                // Accept all SSL certificates (for demo purposes only)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Connect to Gmail SMTP using STARTTLS on port 587.
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // Authenticate with your Gmail credentials.
                await client.AuthenticateAsync(_emailSettings.GmailUser, _emailSettings.GmailAppPassword);

                // Send the email.
                await client.SendAsync(message);

                // Disconnect gracefully.
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new System.Net.Mail.SmtpClient(_configuration["Smtp:Host"])
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
            var smtpClient = new System.Net.Mail.SmtpClient("smtp.mailtrap.io");
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
