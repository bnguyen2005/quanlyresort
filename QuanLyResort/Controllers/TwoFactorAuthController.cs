using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;
using System.Security.Claims;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/auth/2fa")]
[Authorize]
public class TwoFactorAuthController : ControllerBase
{
    private readonly ITwoFactorAuthService _twoFactorService;
    private readonly ILogger<TwoFactorAuthController> _logger;

    public TwoFactorAuthController(
        ITwoFactorAuthService twoFactorService,
        ILogger<TwoFactorAuthController> logger)
    {
        _twoFactorService = twoFactorService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateSecret()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var secret = await _twoFactorService.GenerateSecretAsync(userId);
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            // Generate QR code URI
            var qrCodeUri = $"otpauth://totp/Resort%20Deluxe:{Uri.EscapeDataString(email)}?secret={secret}&issuer=Resort%20Deluxe";

            // Generate QR code image
            var qrCodeBytes = await _twoFactorService.GetQrCodeAsync(userId, email);

            return Ok(new
            {
                secret,
                qrCodeUri,
                qrCodeImage = qrCodeBytes != null ? Convert.ToBase64String(qrCodeBytes) : null,
                message = "Scan QR code with authenticator app"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating 2FA secret");
            return StatusCode(500, new { message = "Error generating 2FA secret", error = ex.Message });
        }
    }

    [HttpPost("enable")]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var success = await _twoFactorService.EnableTwoFactorAsync(userId, request.Code);
            if (!success)
            {
                return BadRequest(new { message = "Invalid code. Please try again." });
            }

            // Get recovery codes
            var recoveryCodes = await _twoFactorService.GenerateRecoveryCodesAsync(userId);
            var codes = recoveryCodes.Split(',');

            return Ok(new
            {
                message = "2FA enabled successfully",
                recoveryCodes = codes,
                warning = "Save these recovery codes in a safe place. You will need them if you lose access to your authenticator app."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA");
            return StatusCode(500, new { message = "Error enabling 2FA", error = ex.Message });
        }
    }

    [HttpPost("verify")]
    [AllowAnonymous] // Used during login
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        try
        {
            var userId = request.UserId;
            if (userId == 0)
                return BadRequest(new { message = "User ID required" });

            var isValid = await _twoFactorService.VerifyCodeAsync(userId, request.Code);
            if (!isValid)
            {
                // Try recovery code
                var isRecoveryValid = await _twoFactorService.VerifyRecoveryCodeAsync(userId, request.Code);
                if (isRecoveryValid)
                {
                    return Ok(new { message = "Recovery code verified", isRecoveryCode = true });
                }

                return BadRequest(new { message = "Invalid code" });
            }

            return Ok(new { message = "Code verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code");
            return StatusCode(500, new { message = "Error verifying code", error = ex.Message });
        }
    }

    [HttpPost("disable")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var success = await _twoFactorService.DisableTwoFactorAsync(userId, request.Password);
            if (!success)
            {
                return BadRequest(new { message = "Failed to disable 2FA" });
            }

            return Ok(new { message = "2FA disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return StatusCode(500, new { message = "Error disabling 2FA", error = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var isEnabled = await _twoFactorService.IsTwoFactorEnabledAsync(userId);
            return Ok(new { enabled = isEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            return StatusCode(500, new { message = "Error getting status", error = ex.Message });
        }
    }

    [HttpGet("recovery-codes")]
    public async Task<IActionResult> GetRecoveryCodes()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var codes = await _twoFactorService.GenerateRecoveryCodesAsync(userId);
            var codeList = codes.Split(',');

            return Ok(new
            {
                recoveryCodes = codeList,
                warning = "Save these codes in a safe place. Old codes are no longer valid."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recovery codes");
            return StatusCode(500, new { message = "Error generating recovery codes", error = ex.Message });
        }
    }
}

public class EnableTwoFactorRequest
{
    public string Code { get; set; } = string.Empty;
}

public class VerifyCodeRequest
{
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class DisableTwoFactorRequest
{
    public string Password { get; set; } = string.Empty;
}

