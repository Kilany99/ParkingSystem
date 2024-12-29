using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Helpers;
using ParkingSystem.Models;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.PaymentDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(ProcessPaymentDto dto)
        {
            var result = await _paymentService.ProcessPaymentAsync(dto.ReservationId, dto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPaymentDetails(int id)
        {
            var payment = await _paymentService.GetPaymentDetailsAsync(id);
            return Ok(payment);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetMyPayments()
        {
            var userId = User.GetUserId();
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }
    }
}
