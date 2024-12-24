using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Helpers;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.UserDtos;

namespace ParkingSystem.Services
{
    public interface IUserService
    {
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
        /*
                public async Task<User> Authenticate(string email, string password)
                {
                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                        return null;

                    var user = await _context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Email == email);

                    if (user == null) return null;

                    if (PasswordHashHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                    {
                        return user;
                    }

                    return null;
                }
                public User Create(User user, string password)
                {
                    // validation
                    if (string.IsNullOrWhiteSpace(password))
                        throw new AppException("Password is required");

                    if (string.IsNullOrWhiteSpace(user.Email))
                        throw new AppException("Email is required");

                    if (_context.Users.Any(x => x.Email == user.Email))
                        throw new AppException("Email \"" + user.Email + "\" is already taken!");

                    byte[] passwordHash, passwordSalt;
                    PasswordHashHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    return user;
                }
        */
        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
        public async Task<UserDto> GetUserById(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            return _mapper.Map<UserDto>(user);
        }
        public async Task<bool> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> GetCurrentUser(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("Current user not found");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = await _context.Users
                .FindAsync(id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            // Update only allowed fields
            user.Name = dto.Name;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }


    }
}
