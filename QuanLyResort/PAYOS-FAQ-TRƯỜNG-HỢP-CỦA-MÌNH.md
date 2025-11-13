# 📋 PayOs FAQ - Các Trường Hợp Của Mình

**Nguồn:** [PayOs FAQ](https://payos.vn/docs/faq/)

## ✅ Các Trường Hợp Đã Gặp

### 1. ✅ Signature Là Gì Và Khi Nào Dùng Đến

**Trạng thái:** Đã implement đầy đủ

**Code hiện tại:**

#### Tạo Payment Link (PayOsService.cs)
```csharp
// PayOs signature format: FIXED ORDER (not alphabetical!)
// Format: amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}
var signatureString = $"amount={amountLong}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
var signature = ComputeHmacSha256(signatureString, _checksumKey);
```

**✅ Đã đúng:**
- Sử dụng 5 trường: `amount`, `orderCode`, `description`, `returnUrl`, `cancelUrl`
- Format: `amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}`
- Dùng HMAC-SHA256 với ChecksumKey

#### Verify Webhook (PayOsWebhookService.cs)
```csharp
// PayOs signature format: HMAC-SHA256 của data
var dataStr = dto.Data != null 
    ? $"{dto.Data.TransactionId}{dto.Data.Amount}{dto.Data.Description}{dto.Data.AccountNumber}{dto.Code}"
    : $"{dto.Code}{dto.Desc}";
var computedSignature = ComputeHmacSha256(dataStr, checksumKey);
```

**⚠️ Lưu ý:**
- Hiện tại `VerifySignature=false` (tắt verification)
- Có thể bật lại khi cần thiết

### 2. ✅ Lỗi "Mã Kiểm Tra(Signature) Không Hợp Lệ"

**Trạng thái:** Đã gặp và đã fix

**Lịch sử:**
- Đã gặp lỗi: `Code: 201, Desc: Mã kiểm tra(signature) không hợp lệ`
- Nguyên nhân: ChecksumKey không đúng hoặc không khớp
- Đã fix: Cập nhật ChecksumKey từ PayOs Dashboard

**Giải pháp đã áp dụng:**
1. ✅ Kiểm tra ChecksumKey từ PayOs Dashboard
2. ✅ Cập nhật `BankWebhook__PayOs__ChecksumKey` trên Railway
3. ✅ Redeploy service

**File liên quan:**
- `FIX-PAYOS-SIGNATURE-ERROR.md`
- `CAP-NHAT-PAYOS-MERCHANT-MOI.md`

### 3. ❌ Nhập Không Đúng Thông Tin CCCD/CMND/MST

**Trạng thái:** Không liên quan

**Lý do:**
- Đã xác thực doanh nghiệp/cá nhân trên PayOs
- Không cần xác thực lại

### 4. ❌ Chuyển Khoản Rồi Nhưng Không Xác Thực Được

**Trạng thái:** Không liên quan

**Lý do:**
- Đã xác thực tài khoản ngân hàng trên PayOs
- Không cần xác thực lại

### 5. ❌ Không Tạo Được Tài Khoản Trên PayOs

**Trạng thái:** Không liên quan

**Lý do:**
- Đã có tài khoản PayOs merchant
- Đã tạo kênh thanh toán

### 6. ⚠️ Số Tài Khoản Trên Link Thanh Toán Không Giống

**Trạng thái:** Cần kiểm tra

**Theo FAQ:**
- Nếu dùng VietQR Pro → Số tài khoản hiển thị là **Số tài khoản ảo**
- Một tài khoản ảo tương ứng với một đơn hàng và số tiền
- Chuyển sai số tài khoản ảo → Đơn hàng không được xác nhận

**Code hiện tại:**
```csharp
// PayOsService.cs - Log account information
_logger.LogInformation("[BACKEND] 🏦 [CreateLink] Account Number: {AccountNumber}, Account Name: {AccountName}", 
    paymentLink.Data.AccountNumber, paymentLink.Data.AccountName);

// Validate account number - phải là 0901329227 (MB Bank)
const string expectedAccountNumber = "0901329227";
if (paymentLink.Data.AccountNumber != expectedAccountNumber)
{
    _logger.LogWarning("[BACKEND] ⚠️ [CreateLink] Account Number mismatch!");
}
```

**✅ Đã xử lý:**
- Code đã log account number
- Code đã validate account number (0901329227 - MB Bank)
- Nếu khác → Log warning

**💡 Lưu ý:**
- Nếu PayOs trả về số tài khoản ảo (virtual account) → Đây là bình thường
- Khách hàng cần chuyển đúng số tài khoản ảo này
- Code đã log để kiểm tra

### 7. ⚠️ Khách Hàng Chuyển Sai Số Tiền

**Trạng thái:** Đã xử lý trong code

**Theo FAQ:**
- Với VietQR Pro:
  - Chuyển sai số tiền → Bị từ chối ở màn hình chuyển khoản
  - Hoặc hệ thống ngân hàng sẽ hoàn tiền và đơn hàng không được xác nhận

**Code hiện tại:**
```csharp
// SimplePaymentController.cs - Verify amount
var estimatedAmount = booking.EstimatedTotalAmount ?? 0;
if (amount > 0 && estimatedAmount > 0)
{
    // Cho phép sai số 10% hoặc chấp nhận nếu amount >= expected
    var diff = Math.Abs(amount - estimatedAmount);
    var maxDiff = estimatedAmount * 0.1m;
    
    // Chấp nhận nếu:
    // 1. Amount >= estimatedAmount (thanh toán đủ hoặc nhiều hơn)
    // 2. Hoặc sai số <= 10%
    if (amount < estimatedAmount && diff > maxDiff)
    {
        _logger.LogWarning("[WEBHOOK] ⚠️ Amount mismatch: Expected={Expected}, Received={Received}", 
            estimatedAmount, amount);
        return BadRequest(new { message = "Số tiền không khớp" });
    }
}
```

**✅ Đã xử lý:**
- Code đã verify amount khi nhận webhook
- Cho phép sai số 10% hoặc chấp nhận nếu amount >= expected
- Nếu sai số quá lớn → Trả về BadRequest

**💡 Lưu ý:**
- Với VietQR Pro, PayOs/ngân hàng sẽ tự động từ chối nếu sai số tiền
- Code chỉ là lớp bảo vệ thêm

## 📊 Tổng Kết

| # | Câu Hỏi | Trạng Thái | Ghi Chú |
|---|---------|-----------|---------|
| 1 | Signature là gì | ✅ Đã implement | Code tạo và verify signature đúng |
| 2 | Lỗi signature không hợp lệ | ✅ Đã fix | Đã cập nhật ChecksumKey |
| 3 | Nhập sai CCCD/CMND/MST | ❌ Không liên quan | Đã xác thực rồi |
| 4 | Chuyển khoản không xác thực được | ❌ Không liên quan | Đã xác thực rồi |
| 5 | Không tạo được tài khoản | ❌ Không liên quan | Đã có tài khoản |
| 6 | Số tài khoản không giống | ⚠️ Cần lưu ý | Code đã log và validate |
| 7 | Chuyển sai số tiền | ✅ Đã xử lý | Code đã verify amount |

## 🔍 Các Vấn Đề Đang Gặp

### 1. PayOs Không Verify Được Railway Webhook URL

**Trạng thái:** Đang gặp vấn đề

**Triệu chứng:**
- PayOs API trả về: `Code: 20 - Webhook url invalid`
- PayOs Dashboard hiển thị: "Webhook url của bạn hiện đang không hoạt động"

**Nguyên nhân:**
- PayOs không verify được Railway domain
- Có thể do PayOs firewall/network

**Giải pháp:**
- Đợi 10-15 phút và thử lại
- Liên hệ PayOs support
- Tạm thời dùng Render URL

### 2. PayOs Chưa Gửi Webhook Sau Khi Thanh Toán

**Trạng thái:** Đang gặp vấn đề

**Triệu chứng:**
- Giao dịch hiển thị "Chờ thanh toán" trên website
- PayOs chưa gửi webhook về Railway

**Nguyên nhân:**
- PayOs chưa verify được webhook URL
- PayOs không gửi webhook nếu URL chưa được verify

**Giải pháp:**
- Fix vấn đề verify webhook URL (xem trên)
- Hoặc update booking status manually

## 📋 Checklist

- [x] ✅ Đã implement signature cho tạo payment link
- [x] ✅ Đã implement verify signature cho webhook (tắt verification)
- [x] ✅ Đã fix lỗi "Mã kiểm tra(signature) không hợp lệ"
- [x] ✅ Đã log và validate account number
- [x] ✅ Đã verify amount khi nhận webhook
- [ ] ⚠️ PayOs chưa verify được Railway webhook URL
- [ ] ⚠️ PayOs chưa gửi webhook sau khi thanh toán

## 💡 Khuyến Nghị

1. **Bật signature verification khi production:**
   - Hiện tại `VerifySignature=false` (development mode)
   - Nên bật lại khi deploy production để bảo mật

2. **Kiểm tra số tài khoản ảo:**
   - Nếu PayOs trả về số tài khoản ảo → Đây là bình thường
   - Đảm bảo khách hàng chuyển đúng số tài khoản ảo

3. **Fix vấn đề verify webhook URL:**
   - Liên hệ PayOs support về Railway domain
   - Hoặc dùng Render URL tạm thời

## 🔗 Links Quan Trọng

- **PayOs FAQ:** https://payos.vn/docs/faq/
- **PayOs Dashboard:** https://payos.vn
- **PayOs Support:** support@payos.vn
- **Fix Signature Error:** `FIX-PAYOS-SIGNATURE-ERROR.md`
- **Verify Webhook:** `KIEM-TRA-PAYOS-VERIFY.md`

