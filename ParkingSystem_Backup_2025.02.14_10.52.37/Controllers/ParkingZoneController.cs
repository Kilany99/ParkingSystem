using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Enums;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class ParkingZoneController : ControllerBase
    {
        private readonly IParkingZoneService _parkingZoneService;
        private readonly ILogger<ParkingZoneService> _logger;

        public ParkingZoneController(IParkingZoneService parkingZoneService,ILogger<ParkingZoneService> logger)
        {
            _parkingZoneService = parkingZoneService;
            _logger = logger;
        }

        [HttpPost]
       // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingZoneDto>> CreateZone(CreateParkingZoneDto dto)
        {
            try
            {
                if (dto.HourlyRate <= 0)
                    return BadRequest("Hourly rate must be greater than zero");

                var result = await _parkingZoneService.CreateZoneAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parking zone");
                return StatusCode(500, "An error occurred while creating the parking zone");
            }
        }
        [HttpGet]
      //  [Authorize(Roles = "Admin")]

        public async Task<ActionResult<IEnumerable<ParkingZoneDto>>> GetAllZones()
        {
            var zones = await _parkingZoneService.GetAllZonesAsync();
            return Ok(zones);
        }

        [HttpGet("{id}/status")]
       // [Authorize(Roles = "Admin")]

        public async Task<ActionResult<ParkingZoneStatusDto>> GetZoneStatus(int id)
        {
            var status = await _parkingZoneService.GetZoneStatusAsync(id);
            return Ok(status);
        }

        [HttpGet("{id}/available-spots")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<IEnumerable<ParkingSpotDto>>> GetSpots(int id, SpotStatus status)
        {
            var spots = await _parkingZoneService.GetSpotsAsync(id,status);
            return Ok(spots);
        }
        [HttpGet("{id}/all-spots")]
        public async Task<ActionResult<IEnumerable<ParkingSpotDto>>> GetAllSpots(int id)
        {
            var spots = await _parkingZoneService.GetAllSpotsAsync(id);
            return Ok(spots);
        }


        [HttpGet("get-all-cars-in-zone")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingZoneDto>> GetCarsInZone(int parkingZoneId)
        {
            try
            {

                var cars = await _parkingZoneService.GetCarsInZoneAsync(parkingZoneId);
                return Ok(cars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cars in the parking zone:{parkingZoneId}");
                return StatusCode(500, $"Error getting cars in the parking zone:{parkingZoneId}");
            }
        }



    }
}
