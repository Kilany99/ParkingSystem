using ParkingSystem.DTOs;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ParkingSystem.Services
{
    public class RabbitMQPublisherService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQPublisherService()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost", 
                UserName = "guest",
                Password = "guest"
            };
        }

        public void PublishReservationCreatedEvent(ReservationCreatedEvent reservationEvent)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "reservation_created_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var message = JsonSerializer.Serialize(reservationEvent);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "reservation_created_queue",
                                     basicProperties: properties,
                                     body: body);
            }
        }
    }
}
