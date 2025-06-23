using RabbitMQ.Client;
using RabbitMQ.Client.Events;  
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkingSystem.Services;
using ParkingSystem.DTOs.Events;

namespace ParkingSystem.Consumers
{
    public class ReservationCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        public ReservationCreatedConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "reservation_created_queue",
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
                var reservationEvent = JsonSerializer.Deserialize<ReservationCreatedEvent>(message) ??
                throw new Exception("An error occured");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    string emailBody = $@"
                    <p>Thank you for using Easy Park!</p>
                    <p>Your reservation will be kept on hold for 24 hours until {reservationEvent.CreatedAt.AddHours(24):f}.</p>
                    <p>Please find attached QR code of your reservation.</p>";

                    await emailService.SendNotificationEmailAsync(
                        reservationEvent.Email,
                        "Your Easy Park Reservation",
                        emailBody,
                        reservationEvent.QRCode
                    );
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "reservation_created_queue",
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
