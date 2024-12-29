using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.ParkingZoneDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingZoneController : ControllerBase
    {
        private readonly IParkingZoneService _parkingZoneService;

        public ParkingZoneController(IParkingZoneService parkingZoneService)
        {
            _parkingZoneService = parkingZoneService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingZoneDto>> CreateZone(CreateParkingZoneDto dto)
        {
            var result = await _parkingZoneService.CreateZoneAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingZoneDto>>> GetAllZones()
        {
            var zones = await _parkingZoneService.GetAllZonesAsync();
            return Ok(zones);
        }

        [HttpGet("{id}/status")]
        public async Task<ActionResult<ParkingZoneStatusDto>> GetZoneStatus(int id)
        {
            var status = await _parkingZoneService.GetZoneStatusAsync(id);
            return Ok(status);
        }

        [HttpGet("{id}/available-spots")]
        public async Task<ActionResult<IEnumerable<ParkingSpotDto>>> GetAvailableSpots(int id)
        {
            var spots = await _parkingZoneService.GetAvailableSpotsAsync(id);
            return Ok(spots);
        }
    }
}
