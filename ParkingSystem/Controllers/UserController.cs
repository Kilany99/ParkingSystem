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
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    private IMapper _mapper;


    public UserController(UserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;

    }

   
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("getall")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [Authorize(Policy = "AdminOnly")]
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

    [Authorize(Policy = "AdminOnly")]
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
        var userId = User.GetUserId();
        var user = await _userService.GetCurrentUser(userId);
        return Ok(user);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteUser(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

}

