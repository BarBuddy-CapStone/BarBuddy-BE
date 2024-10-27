using Application.Interfaces;
using QRCoder;

namespace Infrastructure.QRService
{
    public class QRCodeService : IQRCodeService
    {
        public string GenerateQRCode(Guid guid)
        {
            try
            {
                string guidString = guid.ToString();
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(guidString, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new PngByteQRCode(qrCodeData);
                    return Convert.ToBase64String(qrCode.GetGraphic(20));
                }
            } catch (Exception ex)
            {
                throw new Exception($"Error at Generate QR with error: {ex.Message}");
            }
        }
    }
}
