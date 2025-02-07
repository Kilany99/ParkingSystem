using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ParkingSystem.Helpers
{
    public class QRGenerationHelper
    {
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
    }
}
