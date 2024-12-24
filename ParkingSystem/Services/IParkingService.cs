using ParkingSystem.Data;
using ParkingSystem.Helpers;
using ParkingSystem.Models;

namespace ParkingSystem.Services
{

    public interface IParkingService
    {
        IEnumerable<Parking> GetAll();
        Parking Create(Parking parking);
        Parking GetById(int id);
        void Delete(int id);

    }

    public class ParkingService : IParkingService
    {
        private readonly AppDbContext _context;
        public ParkingService(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Parking> GetAll()
        {
            return _context.Parkings;
        }
        public Parking Create(Parking parking)
        {
            // validation
            if (string.IsNullOrWhiteSpace(parking.ParkingName))
                throw new AppException("Parking name is required");
            if (parking.ParkingCapacity <= 0)
                throw new AppException("Parking Capacity cannot be zero or less!");
            parking.ParkingOccupied = 0;
            parking.IsFull = false;
            _context.Parkings.Add(parking);
            _context.SaveChanges();

            return parking;
        }
        public Parking GetById(int id)
        {
            return _context.Parkings.Find(id);
            
        }

        public void Update(Parking userParam)
        {
            var parking = _context.Parkings.Find(userParam.ParkingId);

            if (parking == null)
                throw new AppException("Parking not found");

            // update parking name if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.ParkingName) && userParam.ParkingName != parking.ParkingName)
            {
                // throw error if the new parking name is already taken
                if (_context.Parkings.Any(x => x.ParkingName == userParam.ParkingName))
                    throw new AppException("Parking " + userParam.ParkingName + " is already taken");

                parking.ParkingName = userParam.ParkingName;
            }

            // update parking properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.Parkingfee))
                parking.Parkingfee = userParam.Parkingfee;

            if (!string.IsNullOrWhiteSpace(userParam.ParkingCapacity.ToString()))
                parking.ParkingCapacity = userParam.ParkingCapacity;

            
            _context.Parkings.Update(parking);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var parking = _context.Parkings.Find(id);
            if (parking != null)
            {
                _context.Parkings.Remove(parking);
                _context.SaveChanges();
            }
            else
                throw new AppException("Parking ID is not vaild");

        }

        private void IsFullParking(Parking parking)
        {
            if (parking.ParkingCapacity - parking.ParkingOccupied == 0)
                parking.IsFull = true;
            else
                parking.IsFull = false;

        }

    }
}
