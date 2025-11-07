namespace QuanLyResort.Services;

public interface IExternalDeviceService
{
    Task<bool> SendToPhoneSystemAsync(string roomNumber, bool activate);
    Task<string?> ReadPassportAsync();
    Task<bool> OpenSafeAsync(string roomNumber);
    Task<bool> ValidateKeyCardAsync(string cardId, string roomNumber);
}

