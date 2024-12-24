using AutoMapper;
using ParkingSystem.DTOs;
using ParkingSystem.Models;

namespace ParkingSystem.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserViewModel>();
            CreateMap<UserRegisterModel, User>();
            CreateMap<UserUpdateModel, User>();

            CreateMap<UpdateReservaion,Reservation>();

            CreateMap<ParkingCreateModel, Parking>();
        }
    }
}
