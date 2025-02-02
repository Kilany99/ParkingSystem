using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.UserDtos;

namespace ParkingSystem.Services
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto dto);

        Task<IEnumerable<UserDto>> GetAllUsers();
        Task<UserDto> GetUserById(int id);
        Task<UserDto> UpdateUser(int id, UpdateUserDto dto);
        Task<bool> DeleteUser(int id);
        Task<UserDto> GetCurrentUser(int userId); // For getting logged-in user details
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers() =>
           _mapper.Map<IEnumerable<UserDto>>(await _context.Users
                .AsNoTracking()
                .ToListAsync());

        public async Task<UserDto> GetUserById(int id) =>
            _mapper.Map<UserDto>(await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id)??
                    throw new KeyNotFoundException($"User with ID {id} not found"));

        public async Task<bool> DeleteUser(int id) =>
            await _context.Users.FindAsync(id) is { } user
            ? await Task.FromResult(_context.Users.Remove(user) != null && await _context.SaveChangesAsync() > 0)
                : false;
        public async Task<UserDto> GetCurrentUser(int userId) =>
            _mapper.Map<UserDto>(await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId)??
                    throw new KeyNotFoundException("Current user not found"));


        public async Task<UserDto> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _context.Users
                .FindAsync(id)??
                    throw new KeyNotFoundException($"User with ID {id} not found");

            // Update only allowed fields
            user.Name = dto.Name;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }


    }
}
