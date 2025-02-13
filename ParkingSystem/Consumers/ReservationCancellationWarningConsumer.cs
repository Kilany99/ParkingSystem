using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

using ParkingSystem.Services;
using ParkingSystem.DTOs.Events;

namespace ParkingSystem.Consumers
{
    public class ReservationCancellationWarningConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        public ReservationCancellationWarningConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "reservation_cancellation_warning_queue",
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
                var reservationEvent = JsonSerializer.Deserialize<ReservationCancellationWarningEvent>(message) ?? throw new Exception("An error occurred");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    string emailBody = $@"
                    <p>Your reservation is scheduled to be cancelled in 1 hour it is valid until {reservationEvent.CancellationTime:f}</p>
                    <p>Please make sure to confirm or modify your reservation if needed.</p>";

                    await emailService.SendNotificationEmailAsync(
                        reservationEvent.Email,
                        "Your Reservation Will Be Cancelled Soon",
                        emailBody
                    );
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "reservation_cancellation_warning_queue",
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
