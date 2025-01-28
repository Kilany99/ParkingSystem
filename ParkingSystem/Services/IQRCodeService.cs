using System.Security.Cryptography;
using System.Text;

namespace ParkingSystem.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(int reservationId, int userId, DateTime timestamp);
        bool ValidateQRCode(string qrCode);
        (int reservationId, int userId, DateTime timestamp) DecodeQRCode(string qrCode);
    }

    public class QRCodeService : IQRCodeService
    {
        private readonly string _secretKey;

        public QRCodeService(IConfiguration configuration)
        {
            _secretKey = configuration["QRCode:SecretKey"]
                ?? throw new ArgumentNullException("QR Code secret key not configured");
        }

        public string GenerateQRCode(int reservationId, int userId, DateTime timestamp)
        {
            // Create a unique string combining multiple factors
            var data = $"{reservationId}:{userId}:{timestamp.Ticks}";

            // Add a hash for security
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var hashString = Convert.ToBase64String(hash);

                // Combine data and hash
                var qrData = $"{data}:{hashString}";

                // Convert to Base64 and make URL-safe
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(qrData))
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");
            }
        }

        public bool ValidateQRCode(string qrCode)
        {
            try
            {
                // Restore padding
                var padding = 4 - (qrCode.Length % 4);
                if (padding != 4) qrCode += new string('=', padding);

                // Replace URL-safe characters
                qrCode = qrCode.Replace("-", "+").Replace("_", "/");

                var decodedBytes = Convert.FromBase64String(qrCode);
                var decodedString = Encoding.UTF8.GetString(decodedBytes);

                var parts = decodedString.Split(':');
                if (parts.Length != 4) return false;

                var data = $"{parts[0]}:{parts[1]}:{parts[2]}";
                var providedHash = parts[3];

                // Verify hash
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
                {
                    var computedHash = Convert.ToBase64String(
                        hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
                    return computedHash == providedHash;
                }
            }
            catch
            {
                return false;
            }
        }

        public (int reservationId, int userId, DateTime timestamp) DecodeQRCode(string qrCode)
        {
            if (!ValidateQRCode(qrCode))
                throw new InvalidOperationException("Invalid QR code");

            // Restore padding
            var padding = 4 - (qrCode.Length % 4);
            if (padding != 4) qrCode += new string('=', padding);

            // Replace URL-safe characters
            qrCode = qrCode.Replace("-", "+").Replace("_", "/");

            var decodedBytes = Convert.FromBase64String(qrCode);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);

            var parts = decodedString.Split(':');
            return (int.Parse(parts[0]),    //resId
                    int.Parse(parts[1]),    //UserId
                    new DateTime(long.Parse(parts[2])) //timeStamp to be used for qr expriry validation
                    );
        }
    }

}
