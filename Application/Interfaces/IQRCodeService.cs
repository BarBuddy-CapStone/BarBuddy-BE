namespace Application.Interfaces
{
    public interface IQRCodeService
    {
        string GenerateQRCode(Guid guid);
    }
}