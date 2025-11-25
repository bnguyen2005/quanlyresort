namespace QuanLyResort.Services;

public interface ITwoFactorAuthService
{
    Task<string> GenerateSecretAsync(int userId);
    Task<bool> VerifyCodeAsync(int userId, string code);
    Task<bool> EnableTwoFactorAsync(int userId, string code);
    Task<bool> DisableTwoFactorAsync(int userId, string password);
    Task<bool> IsTwoFactorEnabledAsync(int userId);
    Task<string> GenerateRecoveryCodesAsync(int userId);
    Task<bool> VerifyRecoveryCodeAsync(int userId, string code);
    Task<byte[]?> GetQrCodeAsync(int userId, string email);
}

