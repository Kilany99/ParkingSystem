using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

using ParkingSystem.Services;
using ParkingSystem.DTOs.Events;

namespace ParkingSystem.Consumers
{
    public class ReservationCancelledConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<ReservationCancelledConsumer> _logger;
        public ReservationCancelledConsumer(IServiceScopeFactory scopeFactory ,ILogger<ReservationCancelledConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "reservation_cancelled_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var reservationEvent = JsonSerializer.Deserialize<ReservationCancelledEvent>(message) ?? throw new Exception("An error occurred");

                using (var scope = _scopeFactory.CreateScope())
                {
                    _logger.LogInformation("Sending notification for cancellation");
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    string emailBody = $@"
                    <p>Your reservation has been cancelled.</p>
                    <p>If this was a mistake, please rebook your reservation.</p>";

                    await emailService.SendNotificationEmailAsync(
                        reservationEvent.Email,
                        "Your Easy Park Reservation Cancelled",
                        emailBody
                    );
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "reservation_cancelled_queue",
                                 autoAck: false,
                                 consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
