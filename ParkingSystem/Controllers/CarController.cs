using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Helpers;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.CarDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize]
    public class CarController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarController(ICarService carService)
        {
            _carService = carService;
        }
        [HttpPost]
        public async Task<ActionResult<CarDto>> AddCar(CreateCarDto dto)
        {
            var userId = User.GetUserId();
            var result = await _carService.AddCarAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarDto>>> GetMyCars()
        {
            var userId = User.GetUserId();
            var cars = await _carService.GetUserCarsAsync(userId);
            return Ok(cars);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CarDto>> UpdateCar(int id, UpdateCarDto dto)
        {
            var userId = User.GetUserId();
            var result = await _carService.UpdateCarAsync(userId, id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var userId = User.GetUserId();
            var result = await _carService.DeleteCarAsync(userId, id);
            return result ? NoContent() : NotFound();
        }

    }
}
