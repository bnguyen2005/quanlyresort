using System.Text.Json.Serialization;

namespace QuanLyResort.Controllers;

/// <summary>
/// Request model cho webhook đơn giản (Simple format)
/// Hỗ trợ cả Simple format và SePay format
/// Format SePay thực tế:
/// {
///     "id": 92704,
///     "gateway": "Vietcombank",
///     "transactionDate": "2023-03-25 14:02:37",
///     "accountNumber": "0123499999",
///     "code": null,
///     "content": "chuyen tien mua iphone",
///     "transferType": "in",
///     "transferAmount": 2277000,
///     "accumulated": 19077000,
///     "subAccount": null,
///     "referenceCode": "MBVCB.3278907687",
///     "description": ""
/// }
/// </summary>
public class SimpleWebhookRequest
{
    // SePay format fields (theo format thực tế từ SePay)
    [JsonPropertyName("id")]
    public int? Id { get; set; } // ID giao dịch trên SePay (ví dụ: 92704)
    
    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; } // Brand name của ngân hàng (ví dụ: "Vietcombank")
    
    [JsonPropertyName("transactionDate")]
    public string? TransactionDate { get; set; } // Thời gian xảy ra giao dịch (ví dụ: "2023-03-25 14:02:37")
    
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; } // Số tài khoản ngân hàng (ví dụ: "0123499999")
    
    [JsonPropertyName("code")]
    public string? Code { get; set; } // Mã code thanh toán (sepay tự nhận diện, có thể null)
    
    [JsonPropertyName("content")]
    public string? Content { get; set; } // Nội dung chuyển khoản (ví dụ: "BOOKING4")
    
    [JsonPropertyName("transferType")]
    public string? TransferType { get; set; } // Loại giao dịch: "in" (tiền vào), "out" (tiền ra)
    
    [JsonPropertyName("transferAmount")]
    public decimal? TransferAmount { get; set; } // Số tiền giao dịch (ví dụ: 2277000)
    
    [JsonPropertyName("accumulated")]
    public decimal? Accumulated { get; set; } // Số dư tài khoản (lũy kế) (ví dụ: 19077000)
    
    [JsonPropertyName("subAccount")]
    public string? SubAccount { get; set; } // Tài khoản ngân hàng phụ (tài khoản định danh), có thể null
    
    [JsonPropertyName("referenceCode")]
    public string? ReferenceCode { get; set; } // Mã tham chiếu của tin nhắn sms (ví dụ: "MBVCB.3278907687")
    
    [JsonPropertyName("description")]
    public string? Description { get; set; } // Toàn bộ nội dung tin nhắn sms (có thể rỗng)
    
    // Legacy fields (để tương thích với format cũ)
    public decimal Amount { get; set; } // Số tiền (fallback nếu không có transferAmount)
    public string? TransactionId { get; set; } // Mã giao dịch (fallback nếu không có id)
    public string? BankName { get; set; } // Tên ngân hàng (fallback nếu không có gateway)
}

/// <summary>
/// Request model cho PayOs webhook (PayOs format)
/// Format từ PayOs API documentation
/// </summary>
public class PayOsWebhookRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty; // "00" = success
    
    [JsonPropertyName("desc")]
    public string Desc { get; set; } = string.Empty; // "success"
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("data")]
    public PayOsWebhookData? Data { get; set; }
    
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>
/// Data trong PayOs webhook
/// Format theo PayOs API documentation: https://payos.vn/docs/api/
/// </summary>
public class PayOsWebhookData
{
    [JsonPropertyName("orderCode")]
    public long? OrderCode { get; set; } // Order code (PayOs gửi long, ví dụ: 123)
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; } // Số tiền (ví dụ: 3000)
    
    [JsonPropertyName("description")]
    public string? Description { get; set; } // Mô tả (có thể chứa booking ID: "BOOKING7" hoặc "VQRIO123")
    
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; } // Số tài khoản (ví dụ: "12345678")
    
    [JsonPropertyName("reference")]
    public string? Reference { get; set; } // Mã tham chiếu giao dịch (ví dụ: "TF230204212323")
    
    [JsonPropertyName("transactionDateTime")]
    public string? TransactionDateTime { get; set; } // Thời gian giao dịch (ví dụ: "2023-02-04 18:25:00")
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; } // Loại tiền tệ (ví dụ: "VND")
    
    [JsonPropertyName("paymentLinkId")]
    public string? PaymentLinkId { get; set; } // ID của payment link (ví dụ: "124c33293c43417ab7879e14c8d9eb18")
    
    // Các trường nested trong data (theo PayOs API documentation)
    [JsonPropertyName("code")]
    public string? Code { get; set; } // Code trong data (ví dụ: "00")
    
    [JsonPropertyName("desc")]
    public string? Desc { get; set; } // Mô tả trong data (ví dụ: "Thành công")
    
    // Thông tin tài khoản đối tác (counter account)
    [JsonPropertyName("counterAccountBankId")]
    public string? CounterAccountBankId { get; set; }
    
    [JsonPropertyName("counterAccountBankName")]
    public string? CounterAccountBankName { get; set; }
    
    [JsonPropertyName("counterAccountName")]
    public string? CounterAccountName { get; set; }
    
    [JsonPropertyName("counterAccountNumber")]
    public string? CounterAccountNumber { get; set; }
    
    // Thông tin tài khoản ảo (virtual account)
    [JsonPropertyName("virtualAccountName")]
    public string? VirtualAccountName { get; set; }
    
    [JsonPropertyName("virtualAccountNumber")]
    public string? VirtualAccountNumber { get; set; }
}

/// <summary>
/// Request để tạo PayOs payment link
/// </summary>
public class CreatePaymentLinkRequest
{
    public int BookingId { get; set; }
}

/// <summary>
/// Request để tạo PayOs payment link cho restaurant order
/// </summary>
public class CreateRestaurantPaymentLinkRequest
{
    public int OrderId { get; set; }
}
