using ParkingSystem.DTOs;
using ParkingSystem.DTOs.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ParkingSystem.Publishers
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
            PublishEvent("reservation_created_queue", reservationEvent);
        }

        public void PublishReservationCanceledEvent(ReservationCancelledEvent reservationEvent)
        {
            PublishEvent("reservation_canceled_queue", reservationEvent);
        }

        public void PublishReservationExpiryWarningEvent(ReservationCancellationWarningEvent reservationEvent)
        {
            PublishEvent("reservation_expiry_warning_queue", reservationEvent);
        }

        private void PublishEvent<T>(string queueName, T eventData)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var message = JsonSerializer.Serialize(eventData);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: properties,
                                     body: body);
            }
        }
    }
}
