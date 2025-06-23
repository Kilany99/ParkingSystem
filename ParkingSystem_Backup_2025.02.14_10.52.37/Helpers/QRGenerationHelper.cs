using ParkingSystem.Services;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ParkingSystem.Helpers
{
    public class QRGenerationHelper
    {
        private readonly IQRCodeService _qrCodeService;

        public QRGenerationHelper(IQRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService; ;
        }
        public byte[] GenerateQRCodeImageBytes(string qrCodeString)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                // Create QR code data
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeString, QRCodeGenerator.ECCLevel.Q);

                // Instantiate QRCode object using QRCoder
                QRCode qrCode = new QRCode(qrCodeData);

                // Generate a Bitmap image of the QR code
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Save the Bitmap as a PNG into the MemoryStream
                        qrCodeImage.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }

        public (int reservationId, int userId, DateTime timestamp) DecodeScannedQRCode(byte[] scannedData)
        {
            var qrCodeString = Encoding.UTF8.GetString(scannedData).Trim();

            var decodedBytes = DecodeBase64UrlSafe(qrCodeString);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);

            if (!_qrCodeService.ValidateQRCode(decodedString))
                throw new InvalidOperationException("Invalid QR code");

            return _qrCodeService.DecodeQRCode(decodedString);
        }


        private static byte[] DecodeBase64UrlSafe(string input)
        {
            // Trim whitespace and newlines
            input = input.Trim();

            // Calculate how many padding characters are needed.
            int mod4 = input.Length % 4;
            if (mod4 > 0)
            {
                input += new string('=', 4 - mod4);
            }

            // Replace URL‑safe characters back to standard Base64 characters.
            input = input.Replace("-", "+").Replace("_", "/");

            // Attempt to convert the string to a byte array.
            return Convert.FromBase64String(input);
        }



    }
}
