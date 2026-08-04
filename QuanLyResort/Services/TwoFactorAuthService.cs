using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuanLyResort.Data;
using System.Security.Cryptography;
using OtpNet;
using QRCoder;

namespace QuanLyResort.Services;

public class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly ResortDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwoFactorAuthService> _logger;
    private const string Issuer = "Resort Deluxe";

    public TwoFactorAuthService(
        ResortDbContext context,
        IConfiguration configuration,
        ILogger<TwoFactorAuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateSecretAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Generate a random secret key
            var secret = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secret);

            // Store secret temporarily (will be saved when user enables 2FA)
            // In production, store in a secure way (encrypted)
            user.TwoFactorSecret = base32Secret;
            user.TwoFactorEnabled = false; // Not enabled until verified

            await _context.SaveChangesAsync();

            _logger.LogInformation("[2FA] Generated secret for user {UserId}", userId);
            return base32Secret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[2FA] Error generating secret for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> VerifyCodeAsync(int userId, string code)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            var secretBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
            var totp = new Totp(secretBytes);

            // Verify code (allow time window of ±1 step = 30 seconds)
            var isValid = totp.VerifyTotp(code, out var timeStepMatched, new VerificationWindow(1, 1));

            if (isValid)
            {
                _logger.LogInformation("[2FA] Code verified for user {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("[2FA] Invalid code for user {UserId}", userId);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[2FA] Error verifying code for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> EnableTwoFactorAsync(int userId, string code)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            // Verify the code first
            if (!await VerifyCodeAsync(userId, code))
                return false;

            // Enable 2FA
            user.TwoFactorEnabled = true;
            user.TwoFactorEnabledAt = DateTime.UtcNow;

            // Generate recovery codes
            var recoveryCodes = await GenerateRecoveryCodesAsync(userId);
            user.TwoFactorRecoveryCodes = recoveryCodes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("[2FA] 2FA enabled for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[2FA] Error enabling 2FA for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DisableTwoFactorAsync(int userId, string password)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Verify password (you may want to use your password hashing service)
            // For now, we'll just disable if user is authenticated
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorRecoveryCodes = null;
            user.TwoFactorEnabledAt = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("[2FA] 2FA disabled for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[2FA] Error disabling 2FA for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsTwoFactorEnabledAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.TwoFactorEnabled ?? false;
    }

    public async Task<string> GenerateRecoveryCodesAsync(int userId)
    {
        var codes = new List<string>();
        var random = new Random();

        for (int i = 0; i < 10; i++)
        {
            // Generate 8-character recovery code
            var code = string.Empty;
            for (int j = 0; j < 8; j++)
            {
                code += random.Next(0, 10).ToString();
            }
            codes.Add(code);
        }

        var recoveryCodes = string.Join(",", codes);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.TwoFactorRecoveryCodes = recoveryCodes;
            await _context.SaveChangesAsync();
        }

        return recoveryCodes;
    }

    public async Task<bool> VerifyRecoveryCodeAsync(int userId, string code)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorRecoveryCodes))
                return false;

            var codes = user.TwoFactorRecoveryCodes.Split(',');
            var index = Array.IndexOf(codes, code);

            if (index >= 0)
            {
                // Remove used recovery code
                var updatedCodes = codes.Where((c, i) => i != index).ToList();
                user.TwoFactorRecoveryCodes = updatedCodes.Any() ? string.Join(",", updatedCodes) : null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("[2FA] Recovery code used for user {UserId}", userId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[2FA] Error verifying recovery code for user {UserId}", userId);
            return false;
        }
    }

    public async Task<byte[]?> GetQrCodeAsync(int userId, string email)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return null;

            // Generate QR code URI
            var secretBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
            var totp = new Totp(secretBytes);
            var uri = $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(email)}?secret={user.TwoFactorSecret}&issuer={Uri.EscapeDataString(Issuer)}";

            // Generate QR code image using QRCoder
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            _logger.LogInformation("[2FA] QR code generated for user {UserId}", userId);
            return qrCodeBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[2FA] Error generating QR code for user {UserId}", userId);
            return null;
        }
    }
}

