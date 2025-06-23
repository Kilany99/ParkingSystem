using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.DTOs.Events;
using ParkingSystem.Enums;
using ParkingSystem.Models;
using ParkingSystem.Publishers;

namespace ParkingSystem.BackgroundServices
{
    public class ReservationCancellationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationCancellationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // checks every 30 mins

        public ReservationCancellationService(IServiceScopeFactory scopeFactory, ILogger<ReservationCancellationService> logger)
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
                        // Find reservations that are still reserved and older than 24 hours.
                        var expiredReservations = await dbContext.Reservations
                           .Include(r => r.ParkingSpot)
                           .Include(r => r.User) // user info is loaded
                           .Where(r => r.Status == SessionStatus.Reserved &&
                                       EF.Functions.DateDiffHour(r.CreatedAt, DateTime.UtcNow) >= 24)
                           .ToListAsync(stoppingToken);

                        if (expiredReservations.Any())
                        {
                            foreach (var reservation in expiredReservations)
                            {
                                // Cancel the reservation.
                                reservation.Status = SessionStatus.Cancelled;
                                reservation.IsPaid = false;
                                // Release the parking spot.
                                if (reservation.ParkingSpot != null)
                                {
                                    reservation.ParkingSpot.Status = SpotStatus.Available;
                                    reservation.ParkingSpot.ReservationId = null;
                                }
                                var reservationEvent = new ReservationCancelledEvent
                                {
                                    ReservationId = reservation.Id,
                                    Email = reservation.User.Email,  // Retrieve user email
                                    CancelledAt = DateTime.UtcNow,
                                    ParkingZoneName = reservation.ParkingSpot?.ParkingZone.Name??""
                                };

                                rabbitMQPublisherService.PublishReservationCanceledEvent(reservationEvent);

                            }

                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"{expiredReservations.Count} expired reservations have been cancelled.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cancelling expired reservations.");
                }

                // Wait for the next check interval.
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
