namespace QuanLyResort.Services;

public class DummyExternalDeviceService : IExternalDeviceService
{
    private readonly ILogger<DummyExternalDeviceService> _logger;

    public DummyExternalDeviceService(ILogger<DummyExternalDeviceService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendToPhoneSystemAsync(string roomNumber, bool activate)
    {
        // TODO: Integrate with actual phone system
        _logger.LogInformation($"[DUMMY] Phone system: Room {roomNumber} - Activate: {activate}");
        await Task.Delay(100); // Simulate API call
        return true;
    }

    public async Task<string?> ReadPassportAsync()
    {
        // TODO: Integrate with passport scanner
        _logger.LogInformation("[DUMMY] Passport scanner: Reading passport...");
        await Task.Delay(100); // Simulate scan
        return "DUMMY_PASSPORT_12345";
    }

    public async Task<bool> OpenSafeAsync(string roomNumber)
    {
        // TODO: Integrate with electronic safe system
        _logger.LogInformation($"[DUMMY] Safe system: Opening safe in room {roomNumber}");
        await Task.Delay(100); // Simulate API call
        return true;
    }

    public async Task<bool> ValidateKeyCardAsync(string cardId, string roomNumber)
    {
        // TODO: Integrate with key card system
        _logger.LogInformation($"[DUMMY] Key card system: Validating card {cardId} for room {roomNumber}");
        await Task.Delay(100); // Simulate validation
        return true;
    }
}

