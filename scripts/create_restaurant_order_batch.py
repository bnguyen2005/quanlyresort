#!/usr/bin/env python3
"""
Script để tạo nhiều đơn đặt món và thanh toán (batch)
Sử dụng: python create_restaurant_order_batch.py [số_lượng_đơn]
"""

import requests
import json
import sys
import random
from datetime import datetime, timedelta

# Cấu hình
BASE_URL = "https://quanlyresort-e0a8.onrender.com"  # Thay đổi nếu cần
# BASE_URL = "http://localhost:5000"  # Cho local development

def login(email, password):
    """Đăng nhập và lấy token"""
    url = f"{BASE_URL}/api/auth/login"
    payload = {"email": email, "password": password}
    
    try:
        response = requests.post(url, json=payload)
        if response.status_code == 200:
            data = response.json()
            return data.get("token"), data.get("customerId")
        return None, None
    except Exception as e:
        print(f"❌ Lỗi đăng nhập: {e}")
        return None, None

def get_services(token):
    """Lấy danh sách món ăn"""
    url = f"{BASE_URL}/api/services"
    headers = {"Authorization": f"Bearer {token}"}
    
    try:
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            return response.json()
        return []
    except Exception as e:
        print(f"❌ Lỗi lấy dịch vụ: {e}")
        return []

def create_and_pay_order(token, customer_id, services, order_num):
    """Tạo và thanh toán một đơn hàng"""
    # Chọn ngẫu nhiên 1-3 món
    num_items = random.randint(1, 3)
    selected_services = random.sample(services, min(num_items, len(services)))
    
    items = []
    for service in selected_services:
        items.append({
            "serviceId": service.get('serviceId'),
            "quantity": random.randint(1, 3),
            "specialNote": None
        })
    
    # Tạo đơn hàng
    url = f"{BASE_URL}/api/restaurant-orders"
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    payload = {
        "customerId": customer_id,
        "items": items,
        "deliveryAddress": f"Phòng {random.randint(101, 999)}",
        "paymentMethod": random.choice(["QR", "Card", "Cash"])
    }
    
    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code == 201:
            order = response.json()
            order_id = order.get('orderId')
            
            # Thanh toán ngay
            pay_url = f"{BASE_URL}/api/restaurant-orders/{order_id}/pay"
            pay_payload = {
                "paymentMethod": payload["paymentMethod"],
                "transactionId": f"TXN{datetime.now().strftime('%Y%m%d%H%M%S')}{order_num}"
            }
            pay_response = requests.post(pay_url, json=pay_payload, headers=headers)
            
            if pay_response.status_code == 200:
                print(f"✅ Đơn #{order_num}: {order.get('orderNumber')} - {order.get('totalAmount'):,.0f} VNĐ - Đã thanh toán")
                return True
            else:
                print(f"⚠️  Đơn #{order_num}: {order.get('orderNumber')} - Tạo thành công nhưng chưa thanh toán")
                return False
        return False
    except Exception as e:
        print(f"❌ Lỗi đơn #{order_num}: {e}")
        return False

def main():
    num_orders = int(sys.argv[1]) if len(sys.argv) > 1 else 5
    
    print("=" * 60)
    print(f"🍽️  TẠO {num_orders} ĐƠN ĐẶT MÓN VÀ THANH TOÁN")
    print("=" * 60)
    
    # Đăng nhập
    email = input("Email (Enter để dùng mặc định): ").strip() or "customer@example.com"
    password = input("Password (Enter để dùng mặc định): ").strip() or "Customer123!"
    
    print("\n📝 Đang đăng nhập...")
    token, customer_id = login(email, password)
    if not token:
        print("❌ Không thể đăng nhập.")
        sys.exit(1)
    
    # Lấy danh sách món
    print("🍕 Đang lấy danh sách món ăn...")
    services = get_services(token)
    if len(services) < 1:
        print("❌ Không có món ăn.")
        sys.exit(1)
    print(f"✅ Có {len(services)} món ăn")
    
    # Tạo và thanh toán các đơn
    print(f"\n🛒 Đang tạo {num_orders} đơn hàng...\n")
    success_count = 0
    
    for i in range(1, num_orders + 1):
        if create_and_pay_order(token, customer_id, services, i):
            success_count += 1
        # Delay nhỏ để tránh spam
        import time
        time.sleep(0.5)
    
    print("\n" + "=" * 60)
    print(f"✅ Hoàn tất! Đã tạo và thanh toán {success_count}/{num_orders} đơn hàng")
    print("=" * 60)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Đã hủy.")
        sys.exit(0)

