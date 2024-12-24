using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Helpers;
using ParkingSystem.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;

namespace ParkingSystem.Services
{
    public interface IReservaionService
    {
        IEnumerable<Reservation> GetAll();
        Reservation Create(Reservation reservation);
        Reservation GetById(int id);

    }

    public class ReservaionService :IReservaionService
    {
        private readonly AppDbContext _context;
        public ReservaionService(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Reservation> GetAll()
        {
            return _context.Reservations;
        }

        public Reservation Create(Reservation reservation)
        {
            // validation
            if (string.IsNullOrWhiteSpace(reservation.ParkingId.ToString()))
                throw new AppException("Parking is required");

            
            if (_context.Parkings.Any(x => x.ParkingId == reservation.ParkingId)!)  
                throw new AppException("Parking does not exist!");

            if (reservation.EndDate < reservation.StartDate)
                throw new AppException("Invalid Dates!");

            if (IsParkingFull(reservation))
                throw new AppException("Parking Alreay Fully Occupied!");



            GenerateReservationCode(reservation);
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return reservation;
        }



        public Reservation GetById(int id)
        {
            return _context.Reservations.Find(id);
        }

        public void Delete(int id)
        {
            var reservaion = _context.Reservations.Find(id);
            if (reservaion != null)
            {
                _context.Reservations.Remove(reservaion);
                _context.SaveChanges();
            }
        }

        public void Update(Reservation userParam)
        {
            var reservation = _context.Reservations.Find(userParam.UserId);

            if (reservation == null)
                throw new AppException("Reservation not found");

            // update date if it has changed and the parking is not full
            if (userParam.StartDate != reservation.StartDate && userParam.EndDate != reservation.EndDate && !reservation.Parking.IsFull)
            {
                reservation.StartDate = userParam.StartDate;
                reservation.EndDate = userParam.EndDate;
            }

/*
            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.Name))
                user.Name = userParam.Name;

            if (!string.IsNullOrWhiteSpace(userParam.Phone))
                user.Phone = userParam.Phone;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                PasswordHashHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
*/
        }

        private void GenerateReservationCode(Reservation reservation)
        {
            string reservationCode = Guid.NewGuid().ToString(); // Generate a unique code
            var qrCodeWriter = new ZXing.BarcodeWriter<Bitmap>
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 200,
                    Height = 200
                }
            };

            using (var qrCodeImage = qrCodeWriter.Write(reservationCode))
            {
                using (var memoryStream = new MemoryStream())
                {
                    qrCodeImage.Save(memoryStream, ImageFormat.Png);
                    reservation.ReservationCode = memoryStream.ToArray(); // Assuming you have a byte array property for the image
                }
            }
        }

        private bool IsParkingFull(Reservation reservation)
        {
            int parkingId = reservation.ParkingId;
            Parking parking = new Parking();
            parking = _context.Parkings.Find(parkingId);
            if (parking == null)
                throw new AppException("Cannot Find Any Parking registred by this ID");
            if (parking.IsFull)
                return true;
            else
                return false;
        }





    }

}
