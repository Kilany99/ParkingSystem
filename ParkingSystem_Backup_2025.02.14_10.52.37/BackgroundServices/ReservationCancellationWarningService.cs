using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.DTOs.Events;
using ParkingSystem.Enums;
using ParkingSystem.Publishers;


namespace ParkingSystem.BackgroundServices
{
    public class ReservationCancellationWarningService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationCancellationWarningService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Runs every 30 minutes

        public ReservationCancellationWarningService(IServiceScopeFactory scopeFactory, ILogger<ReservationCancellationWarningService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var rabbitMQPublisherService = scope.ServiceProvider.GetRequiredService<RabbitMQPublisherService>();

                        // Find reservations that are still 'Reserved' and will expire within the next 1 hour
                        var expiringReservations = await dbContext.Reservations
                            .Include(r => r.ParkingSpot)
                            .Include(r => r.User) // Ensure user info is loaded
                            .Where(r => r.Status == SessionStatus.Reserved &&
                                        EF.Functions.DateDiffHour(r.CreatedAt, DateTime.UtcNow) >= 23 &&
                                        EF.Functions.DateDiffHour(r.CreatedAt, DateTime.UtcNow) < 24) // Will expire in the next 1 hour
                            .ToListAsync(stoppingToken);

                        if (expiringReservations.Any())
                        {
                            foreach (var reservation in expiringReservations)
                            {
                                // Send cancellation warning email to the user
                                if (reservation.User != null)
                                {
                                    var reservationEvent = new ReservationCancellationWarningEvent
                                    {
                                        Email = reservation.User.Email,
                                        CancellationTime = reservation.CreatedAt.AddHours(24)
                                    };

                                    // Publish event (optional: for other services to react)
                                    rabbitMQPublisherService.PublishReservationExpiryWarningEvent(reservationEvent);

                                    // Log the event
                                    _logger.LogInformation($"Cancellation warning email sent to {reservation.User.Email} for reservation {reservation.Id}.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending cancellation warning emails.");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
