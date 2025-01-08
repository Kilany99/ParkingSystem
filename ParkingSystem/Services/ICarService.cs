using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.CarDtos;

namespace ParkingSystem.Services
{
    public interface ICarService
    {
        Task<CarDto> AddCarAsync(int userId, CreateCarDto dto);
        Task<IEnumerable<CarDto>> GetUserCarsAsync(int userId);

        Task<IEnumerable<CarDto>> getAllCarsAsync();
        Task<CarDto> UpdateCarAsync(int userId, int carId, UpdateCarDto dto);
        Task<bool> DeleteCarAsync(int userId, int carId);
    }
    public class CarService : ICarService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CarService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<CarDto> AddCarAsync(int userId, CreateCarDto dto)
        {
            // Check if plate number already exists
            if (await _context.Cars.AnyAsync(c => c.PlateNumber == dto.PlateNumber))
            {
                throw new InvalidOperationException("Car with this plate number already exists");
            }

            var car = new Car
            {
                UserId = userId,
                PlateNumber = dto.PlateNumber,
                Model = dto.Model,
                Color = dto.Color
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return _mapper.Map<CarDto>(car);
        }

        public async Task<IEnumerable<CarDto>> GetUserCarsAsync(int userId)
        {
            var cars = await _context.Cars
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CarDto>>(cars);
        }
        public async Task<IEnumerable<CarDto>> getAllCarsAsync()
        {
            var cars = await _context.Cars
                .ToListAsync();
            return _mapper.Map<IEnumerable<CarDto>>(cars);
        }

        public async Task<CarDto> UpdateCarAsync(int userId, int carId, UpdateCarDto dto)
        {
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == carId && c.UserId == userId);

            if (car == null)
                throw new KeyNotFoundException("Car not found");

            car.Model = dto.Model;
            car.Color = dto.Color;

            await _context.SaveChangesAsync();

            return _mapper.Map<CarDto>(car);
        }

        public async Task<bool> DeleteCarAsync(int userId, int carId)
        {
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == carId && c.UserId == userId);

            if (car == null)
                return false;

            // Delete related Reservations (ParkingSessions)
            var reservations = await _context.Reservations
                .Where(r => r.CarId == carId)
                .ToListAsync();



            // Remove all related reservations
            _context.Reservations.RemoveRange(reservations);

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return true;
        }

    }

}
