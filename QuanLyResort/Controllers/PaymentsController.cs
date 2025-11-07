using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuanLyResort.Data;
using QuanLyResort.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuanLyResort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ResortDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public PaymentsController(ResortDbContext db, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private string HmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        public class CreateMomoRequest
        {
            public int BookingId { get; set; }
        }

        [HttpPost("momo/create")]
        [Authorize(Roles = "Customer,Admin,FrontDesk,Cashier,Manager")]
        public async Task<IActionResult> CreateMomoPayment([FromBody] CreateMomoRequest req)
        {
            var booking = await _db.Bookings.FindAsync(req.BookingId);
            if (booking == null) return NotFound(new { message = "Booking not found" });

            var momo = _config.GetSection("MoMo");
            var partnerCode = momo["PartnerCode"];
            var accessKey = momo["AccessKey"];
            var secretKey = momo["SecretKey"];
            var endpoint = momo["Endpoint"];
            var returnUrl = momo["ReturnUrl"];
            var ipnUrl = momo["IpnUrl"];

            if (string.IsNullOrWhiteSpace(partnerCode) || partnerCode.StartsWith("YOUR_"))
                return BadRequest(new { message = "MoMo credentials are not configured." });

            var orderId = $"{req.BookingId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var requestId = Guid.NewGuid().ToString("N");
            var amount = (long)((booking.EstimatedTotalAmount ?? 0m));
            if (amount <= 0) amount = 1; // minimal demo amount

            var orderInfo = $"Payment for booking #{req.BookingId}";
            var requestType = "captureWallet";

            // raw signature string by MoMo spec (v2 create)
            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData=&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSha256(rawHash, secretKey);

            var payload = new
            {
                partnerCode,
                partnerName = "RESORT DELUXE",
                storeId = "RESORT-DEV",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                requestType,
                extraData = string.Empty,
                lang = "vi",
                signature
            };

            var client = _httpClientFactory.CreateClient();
            var httpReq = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var httpResp = await client.SendAsync(httpReq);
            var body = await httpResp.Content.ReadAsStringAsync();
            if (!httpResp.IsSuccessStatusCode)
            {
                return StatusCode((int)httpResp.StatusCode, new { message = "MoMo error", body });
            }

            // Relay payUrl to frontend
            return Content(body, "application/json");
        }

        // MoMo returns
        [HttpGet("momo/return")]
        [AllowAnonymous]
        public IActionResult MomoReturn() => Ok(new { message = "Return received", query = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()) });

        // MoMo IPN notify
        [HttpPost("momo/ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> MomoIpn()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            try
            {
                var doc = JsonDocument.Parse(body).RootElement;
                var resultCode = doc.GetProperty("resultCode").GetInt32();
                var orderId = doc.GetProperty("orderId").GetString() ?? string.Empty;
                var bookingIdStr = orderId.Split('-').FirstOrDefault();
                if (int.TryParse(bookingIdStr, out var bookingId))
                {
                    var booking = await _db.Bookings.FindAsync(bookingId);
                    if (booking != null && resultCode == 0)
                    {
                        booking.Status = "Paid";
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch { /* ignore */ }
            return Ok(new { message = "ipn ok" });
        }
    }
}


