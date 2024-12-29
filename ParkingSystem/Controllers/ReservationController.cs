using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Helpers;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.ReservationDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationDto dto)
        {
            var userId = User.GetUserId();
            var result = await _reservationService.CreateReservationAsync(userId, dto);
            return Ok(result);
        }

        [HttpPost("start")]
        public async Task<ActionResult<ReservationDto>> StartParking([FromBody] string qrCode)
        {
            var result = await _reservationService.StartParkingAsync(qrCode);
            return Ok(result);
        }

        [HttpPost("end")]
        public async Task<ActionResult<ReservationDto>> EndParking([FromBody] string qrCode)
        {
            var result = await _reservationService.EndParkingAsync(qrCode);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            var userId = User.GetUserId();
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }

        [HttpGet("{id}/fee")]
        public async Task<ActionResult<decimal>> CalculateFee(int id)
        {
            var fee = await _reservationService.CalculateParkingFeeAsync(id);
            return Ok(fee);
        }
    }
}
