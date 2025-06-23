using AutoMapper;
using ParkingSystem.DTOs;
using ParkingSystem.Models;
using static ParkingSystem.DTOs.CarDtos;
using static ParkingSystem.DTOs.ParkingZoneDtos;
using static ParkingSystem.DTOs.PaymentDtos;
using static ParkingSystem.DTOs.ReservationDtos;
using static ParkingSystem.DTOs.UserDtos;

namespace ParkingSystem.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Map from User to UserDto
            CreateMap<User, UserDto>();

            // Map from UpdateUserDto to User
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => HashPassword(src.Password)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)); // Set the CreatedAt to current time

            CreateMap<Car, CarDto>();
            CreateMap<CreateCarDto, Car>();

            CreateMap<ParkingZone, ParkingZoneDto>();
            CreateMap<ParkingSpot, ParkingSpotDto>()
                .ForMember(dest => dest.CurrentReservation, opt => opt.MapFrom(src => src.CurrentReservation));


            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
                .ForMember(dest => dest.ParkingSpot, opt => opt.MapFrom(src => src.ParkingSpot))
                .ForMember(dest => dest.ParkingZone, opt => opt.MapFrom(src => src.ParkingSpot.ParkingZone));
            CreateMap<Payment, PaymentDto>();

        }
        private string HashPassword(string password) =>
           BCrypt.Net.BCrypt.HashPassword(password);

    }
}
