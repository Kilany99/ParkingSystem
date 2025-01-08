using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Data;
using ParkingSystem.DTOs;
using ParkingSystem.Helpers;
using ParkingSystem.Models;
using ParkingSystem.Services;
using System.Linq;
using System.Threading.Tasks;
using static ParkingSystem.DTOs.UserDtos;

    
[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;


    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    }



    [HttpPost("create")]
    // [Authorize(Policy ="AdminOnly")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }


    // [Authorize(Policy = "AdminOnly")]
    [HttpGet("getall")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

   // [Authorize(Policy = "AdminOnly")]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        try
        {
            var user = await _userService.GetUserById(id);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

   // [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, UpdateUserDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId != id && !User.IsInRole("Admin"))
                return Forbid();

            var user = await _userService.UpdateUser(id, dto);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Getting details for user ID: {UserId}", userId);

            var result = await _userService.GetUserById(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user details");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

   // [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteUser(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

}

