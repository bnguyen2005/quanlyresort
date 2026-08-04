#!/usr/bin/env python3
"""
Script để đặt và thanh toán món ăn
Sử dụng: python create_restaurant_order.py
"""

import requests
import json
import sys
from datetime import datetime, timedelta

# Cấu hình
BASE_URL = "https://quanlyresort-e0a8.onrender.com"  # Thay đổi nếu cần
# BASE_URL = "http://localhost:5000"  # Cho local development

def login(email, password):
    """Đăng nhập và lấy token"""
    url = f"{BASE_URL}/api/auth/login"
    payload = {
        "email": email,
        "password": password
    }
    
    try:
        response = requests.post(url, json=payload)
        if response.status_code == 200:
            data = response.json()
            token = data.get("token")
            customer_id = data.get("customerId")
            print(f"✅ Đăng nhập thành công! Customer ID: {customer_id}")
            return token, customer_id
        else:
            print(f"❌ Lỗi đăng nhập: {response.status_code} - {response.text}")
            return None, None
    except Exception as e:
        print(f"❌ Lỗi khi đăng nhập: {e}")
        return None, None

def get_services(token):
    """Lấy danh sách món ăn/dịch vụ"""
    url = f"{BASE_URL}/api/services"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            services = response.json()
            # Lọc chỉ lấy món ăn (có thể filter theo ServiceType nếu có)
            print(f"✅ Lấy được {len(services)} dịch vụ")
            return services
        else:
            print(f"❌ Lỗi lấy danh sách dịch vụ: {response.status_code}")
            return []
    except Exception as e:
        print(f"❌ Lỗi khi lấy dịch vụ: {e}")
        return []

def create_order(token, customer_id, items, delivery_address=None, payment_method="QR"):
    """Tạo đơn đặt món"""
    url = f"{BASE_URL}/api/restaurant-orders"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    payload = {
        "customerId": customer_id,
        "items": items,
        "deliveryAddress": delivery_address or "Phòng 101",
        "requestedDeliveryTime": (datetime.now() + timedelta(hours=1)).isoformat(),
        "specialRequests": "Giao nhanh",
        "paymentMethod": payment_method
    }
    
    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code == 201:
            order = response.json()
            print(f"✅ Tạo đơn hàng thành công! Order ID: {order.get('orderId')}, Order Number: {order.get('orderNumber')}")
            print(f"   Tổng tiền: {order.get('totalAmount'):,.0f} VNĐ")
            return order
        else:
            print(f"❌ Lỗi tạo đơn hàng: {response.status_code} - {response.text}")
            return None
    except Exception as e:
        print(f"❌ Lỗi khi tạo đơn hàng: {e}")
        return None

def pay_order(token, order_id, payment_method="QR"):
    """Thanh toán đơn hàng"""
    url = f"{BASE_URL}/api/restaurant-orders/{order_id}/pay"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    payload = {
        "paymentMethod": payment_method,
        "transactionId": f"TXN{datetime.now().strftime('%Y%m%d%H%M%S')}",
        "paidAmount": None  # Sẽ lấy từ order
    }
    
    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code == 200:
            result = response.json()
            print(f"✅ Thanh toán thành công!")
            print(f"   Payment Status: {result.get('paymentStatus')}")
            return result
        else:
            print(f"❌ Lỗi thanh toán: {response.status_code} - {response.text}")
            return None
    except Exception as e:
        print(f"❌ Lỗi khi thanh toán: {e}")
        return None

def main():
    print("=" * 60)
    print("🍽️  SCRIPT ĐẶT VÀ THANH TOÁN MÓN ĂN")
    print("=" * 60)
    
    # Thông tin đăng nhập (có thể thay đổi)
    email = input("Nhập email khách hàng (hoặc Enter để dùng mặc định): ").strip()
    if not email:
        email = "customer@example.com"  # Thay đổi email mặc định
    
    password = input("Nhập mật khẩu (hoặc Enter để dùng mặc định): ").strip()
    if not password:
        password = "Customer123!"  # Thay đổi password mặc định
    
    # Đăng nhập
    print("\n📝 Đang đăng nhập...")
    token, customer_id = login(email, password)
    if not token:
        print("❌ Không thể đăng nhập. Vui lòng kiểm tra lại thông tin.")
        sys.exit(1)
    
    # Lấy danh sách món ăn
    print("\n🍕 Đang lấy danh sách món ăn...")
    services = get_services(token)
    if not services:
        print("❌ Không lấy được danh sách món ăn.")
        sys.exit(1)
    
    # Hiển thị danh sách món ăn
    print("\n📋 Danh sách món ăn:")
    for i, service in enumerate(services[:10], 1):  # Hiển thị 10 món đầu
        service_id = service.get('serviceId')
        service_name = service.get('serviceName', 'N/A')
        price = service.get('price', 0)
        print(f"   {i}. {service_name} - {price:,.0f} VNĐ (ID: {service_id})")
    
    # Tạo đơn hàng
    print("\n🛒 Tạo đơn hàng...")
    
    # Chọn món ăn (có thể random hoặc chọn mặc định)
    items = []
    if len(services) >= 2:
        # Lấy 2-3 món đầu tiên
        selected_services = services[:min(3, len(services))]
        for service in selected_services:
            items.append({
                "serviceId": service.get('serviceId'),
                "quantity": 2,  # Số lượng
                "specialNote": f"Ghi chú cho {service.get('serviceName')}"
            })
    else:
        print("❌ Không đủ món ăn để tạo đơn hàng.")
        sys.exit(1)
    
    print(f"   Đang đặt {len(items)} món...")
    order = create_order(token, customer_id, items, delivery_address="Phòng 101", payment_method="QR")
    
    if not order:
        print("❌ Không thể tạo đơn hàng.")
        sys.exit(1)
    
    order_id = order.get('orderId')
    total_amount = order.get('totalAmount', 0)
    
    # Thanh toán đơn hàng
    print(f"\n💳 Đang thanh toán đơn hàng {order_id}...")
    print(f"   Tổng tiền: {total_amount:,.0f} VNĐ")
    
    payment_result = pay_order(token, order_id, payment_method="QR")
    
    if payment_result:
        print("\n" + "=" * 60)
        print("✅ HOÀN TẤT!")
        print("=" * 60)
        print(f"Order ID: {order_id}")
        print(f"Order Number: {order.get('orderNumber')}")
        print(f"Tổng tiền: {total_amount:,.0f} VNĐ")
        print(f"Trạng thái thanh toán: {payment_result.get('paymentStatus', 'Paid')}")
        print("=" * 60)
    else:
        print("\n❌ Không thể thanh toán đơn hàng.")
        print(f"   Đơn hàng đã được tạo với ID: {order_id}")
        print(f"   Bạn có thể thanh toán sau qua API hoặc giao diện web.")

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Đã hủy bởi người dùng.")
        sys.exit(0)
    except Exception as e:
        print(f"\n❌ Lỗi không mong đợi: {e}")
        sys.exit(1)

