using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Attributes;
using ParkingSystem.DTOs;
using ParkingSystem.Enums;
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
        private readonly ILogger<ReservationController> _logger;
        public ReservationController(IReservationService reservationService,
            ILogger<ReservationController> logger,
            IQRCodeService qrCodeService)
        {
            _reservationService = reservationService;
            _logger = logger;
        }
        [Authorize(Roles = "User")]
        [CustomRateLimit("1m", 10)]
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationDto dto)
        {
            try
            {
                var userId = User.GetUserId();

                // Check for active reservations
                if (await _reservationService.HasActiveReservation(dto.CarId))
                {
                    return BadRequest(new
                    {
                        Error = "Car has active reservation"
                    });
                }

                var result = await _reservationService.CreateReservationAsync(userId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(500, "An error occurred while creating the reservation");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("start")]  //Called from parking zone with QR code reader and plate number reader on the entrence of the parking.
        public async Task<ActionResult<ReservationDto>> StartParking([FromBody] ParkingRequest request)
        {
            try
            {
                var result = await _reservationService.StartParkingAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Expired")) 
            {
                return BadRequest("QR code Expired");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("mismatch"))
            {
                return BadRequest("Plate number is not matching");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting parking reservation");
                return StatusCode(500, "An error occurred while start parking reservation "+ ex);

            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("end")] //Called from parking zone with QR code reader and plate number reader on the exit of the parking.
        public async Task<ActionResult<ReservationDto>> EndParking([FromBody] ParkingRequest request, PaymentMethod method)
        {
            try
            {
                var result = await _reservationService.EndParkingAsync(request, method);
                return Ok(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("plate format"))
            {
                return BadRequest("Plate number is not in correct form");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("QR"))
            {
                return BadRequest("Invalid QR code");

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending parking reservation");
                return StatusCode(500, "An error occurred while ending parking reservation " + ex.Message);

            }
        }
        [Authorize(Roles = "User")]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            var userId = User.GetUserId();
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }
        [Authorize(Roles = "User")]
        [HttpGet("{id}/fee")]
        public async Task<ActionResult<decimal>> CalculateFee(int id)
        {
            var fee = await _reservationService.CalculateParkingFeeAsync(id);
            return Ok(fee);
        }

        [Authorize(Roles = "User")]
        [HttpGet("{id}/cnx-fee")]
        public async Task<ActionResult<decimal>> CalculateCancelFee(int id)
        {
            var fee = await _reservationService.CalculateCnxFeeAsync(id);
            return Ok(fee);
        }
        [Authorize(Roles = "User")]
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ReservationDto>> CancelReservation(int id)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _reservationService.CancelReservationAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
                return StatusCode(500, "An error occurred while cancelling the reservation"+ex.Message);
            }
        }

        [HttpGet("get-qr-image")]
        public async Task<IActionResult> GetQRImage(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
            {
                return BadRequest("Invalid QR code.");
            }
            try
            {
                byte[] qrImage = _reservationService.GetQRImage(qrCode);
                string base64Image = Convert.ToBase64String(qrImage);
                return Ok(new { qrCode = $"data:image/png;base64,{base64Image}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating QR code: {ex.Message}");
            }
           

        }
        [Authorize(Roles = "Admin")]
        [HttpGet("today-revenue")]
        public async Task<IActionResult> GetTodayRevenue()
        {
            var revenue = await _reservationService.GetTodayRevenueAsync();
            return Ok(new { todayRevenue = revenue });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("today-activity")]
        public async Task<IActionResult> GetTodayActivity()
        {
            var activity = await _reservationService.GetTodayActivity();
            return Ok(activity);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservationsAsync();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError("an error occured while trying to get reservations " + ex.Message);
                return StatusCode(500,"an error occured while trying to get reservations" + ex.Message);
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-in-parkingzone")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations(int parkingZoneId)
        {
            try
            {
                var reservations = await _reservationService.GetReservationsInParkingZoneAsync(parkingZoneId);
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError("an error occured while trying to get reservations " + ex.Message);
                return StatusCode(500, "an error occured while trying to get reservations" + ex.Message);
            }

        }

    }
}
