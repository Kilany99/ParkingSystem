using ParkingSystem.Enums;

namespace ParkingSystem.DTOs
{
    public class PaymentDtos
    {
        public record ProcessPaymentDto(int ReservationId, decimal Amount, PaymentMethod Method);
        public record PaymentDto(
            int Id,
            decimal Amount,
            PaymentStatus Status,
            PaymentMethod Method,
            DateTime CreatedAt,
            DateTime? CompletedAt
        );
    }
}
