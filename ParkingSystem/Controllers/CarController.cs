using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Helpers;
using ParkingSystem.Services;
using static ParkingSystem.DTOs.CarDtos;

namespace ParkingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            try
            {
                var userId = User.GetUserId();
                var result = await _carService.AddCarAsync(userId, dto);
                return Ok(result);
            }
            catch(InvalidOperationException ex) when (ex.Message.Contains("Invalid")) 
            {
                return StatusCode(500, "Invalid license plate format");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("exists"))
            {
                return StatusCode(500, "Car with this plate number already exists");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
