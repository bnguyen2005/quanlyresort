# 🧪 Test Reviews API Endpoint

## ✅ Endpoint: `/api/reviews`

**URL:** `https://quanlyresort-production.up.railway.app/api/reviews`

**Method:** `GET`

**Authentication:** Không cần (Public endpoint)

## 📋 Cách Test

### 1. Test Trong Trình Duyệt

Mở trình duyệt và vào:
```
https://quanlyresort-production.up.railway.app/api/reviews
```

### 2. Test Bằng cURL

```bash
curl https://quanlyresort-production.up.railway.app/api/reviews
```

### 3. Test Với Query Parameters

#### Lấy reviews của một phòng cụ thể:
```
https://quanlyresort-production.up.railway.app/api/reviews?roomId=1
```

#### Lấy reviews với rating cụ thể (1-5):
```
https://quanlyresort-production.up.railway.app/api/reviews?rating=5
```

#### Giới hạn số lượng reviews:
```
https://quanlyresort-production.up.railway.app/api/reviews?limit=10
```

#### Kết hợp nhiều parameters:
```
https://quanlyresort-production.up.railway.app/api/reviews?roomId=1&rating=5&limit=10
```

## 📊 Response Format

### Thành Công (200 OK):

```json
{
  "reviews": [
    {
      "reviewId": 1,
      "rating": 5,
      "comment": "Phòng rất đẹp và sạch sẽ!",
      "response": "Cảm ơn bạn đã đánh giá!",
      "responseDate": "2025-11-13T10:00:00Z",
      "respondedBy": "admin@resort.test",
      "createdAt": "2025-11-10T08:00:00Z",
      "customerName": "Nguyễn Văn A",
      "customerInitials": "NVA",
      "roomNumber": "101",
      "roomType": "Deluxe"
    }
  ],
  "statistics": {
    "totalReviews": 50,
    "averageRating": 4.5,
    "ratingDistribution": [
      {
        "rating": 5,
        "count": 30
      },
      {
        "rating": 4,
        "count": 15
      },
      {
        "rating": 3,
        "count": 5
      }
    ]
  }
}
```

### Nếu Không Có Reviews:

```json
{
  "reviews": [],
  "statistics": {
    "totalReviews": 0,
    "averageRating": 0.0,
    "ratingDistribution": []
  }
}
```

## 🔍 Query Parameters

| Parameter | Type | Mô Tả | Ví Dụ |
|-----------|------|-------|-------|
| `roomId` | int? | Filter theo ID phòng | `?roomId=1` |
| `rating` | int? | Filter theo rating (1-5) | `?rating=5` |
| `limit` | int | Giới hạn số lượng (mặc định: 50) | `?limit=10` |

## 📝 Lưu Ý

- Endpoint này chỉ trả về reviews **đã được approved** và **visible**
- Reviews được sắp xếp theo thời gian tạo mới nhất
- Statistics được tính dựa trên tất cả reviews approved

## 🧪 Test Cases

### Test 1: Lấy tất cả reviews
```bash
curl https://quanlyresort-production.up.railway.app/api/reviews
```

### Test 2: Lấy reviews của phòng 1
```bash
curl https://quanlyresort-production.up.railway.app/api/reviews?roomId=1
```

### Test 3: Lấy reviews 5 sao
```bash
curl https://quanlyresort-production.up.railway.app/api/reviews?rating=5
```

### Test 4: Lấy 10 reviews mới nhất
```bash
curl https://quanlyresort-production.up.railway.app/api/reviews?limit=10
```

### Test 5: Lấy reviews 5 sao của phòng 1, giới hạn 5
```bash
curl https://quanlyresort-production.up.railway.app/api/reviews?roomId=1&rating=5&limit=5
```

## 🐛 Troubleshooting

### Lỗi: 404 Not Found

**Nguyên nhân:**
- URL sai
- Service chưa start

**Giải pháp:**
1. Kiểm tra URL đúng: `https://quanlyresort-production.up.railway.app/api/reviews`
2. Kiểm tra logs xem service đã start chưa

### Lỗi: 500 Internal Server Error

**Nguyên nhân:**
- Database connection lỗi
- Database chưa có dữ liệu

**Giải pháp:**
1. Kiểm tra logs để xem lỗi cụ thể
2. Đảm bảo database đã được migrate và seed data

### Response Trống (Không Có Reviews)

**Nguyên nhân:**
- Database chưa có reviews
- Tất cả reviews chưa được approved

**Giải pháp:**
1. Kiểm tra database có dữ liệu reviews không
2. Đảm bảo reviews có `IsApproved = true` và `IsVisible = true`

## ✅ Kiểm Tra Response

Sau khi gọi API, kiểm tra:

1. **Status Code:** Phải là `200 OK`
2. **Response có structure:**
   - `reviews`: Array các review objects
   - `statistics`: Object chứa thống kê
3. **Reviews có đầy đủ fields:**
   - `reviewId`, `rating`, `comment`
   - `customerName`, `roomNumber`
   - `createdAt`, etc.

## 🎯 Kết Quả Mong Đợi

Nếu database đã có reviews:
- ✅ Trả về danh sách reviews
- ✅ Có statistics (totalReviews, averageRating, ratingDistribution)
- ✅ Reviews được sắp xếp mới nhất trước

Nếu database chưa có reviews:
- ✅ Trả về `reviews: []`
- ✅ Statistics: `totalReviews: 0`, `averageRating: 0.0`

