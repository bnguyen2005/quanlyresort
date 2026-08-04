#!/usr/bin/env python3
"""
Script test tổng hợp: Đặt phòng + Đặt món ăn để kiểm tra Dashboard và Reports
Sử dụng: python test_dashboard_reports.py
"""

import requests
import json
import sys
import time
from datetime import datetime, timedelta

BASE_URL = "https://quanlyresort-e0a8.onrender.com"

def login(email, password):
    """Đăng nhập"""
    url = f"{BASE_URL}/api/auth/login"
    try:
        response = requests.post(url, json={"email": email, "password": password})
        if response.status_code == 200:
            data = response.json()
            return data.get("token"), data.get("customerId")
        return None, None
    except Exception as e:
        print(f"❌ Lỗi đăng nhập: {e}")
        return None, None

def create_booking(token, customer_id):
    """Tạo đặt phòng"""
    url = f"{BASE_URL}/api/bookings"
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    
    check_in = datetime.now() + timedelta(days=7)
    check_out = check_in + timedelta(days=2)
    
    payload = {
        "customerId": customer_id,
        "requestedRoomType": "Standard",
        "checkInDate": check_in.isoformat(),
        "checkOutDate": check_out.isoformat(),
        "numberOfGuests": 2,
        "source": "Website"
    }
    
    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code == 201:
            booking = response.json()
            return booking
        return None
    except Exception as e:
        print(f"❌ Lỗi tạo booking: {e}")
        return None

def pay_booking(token, booking_id):
    """Thanh toán đặt phòng"""
    url = f"{BASE_URL}/api/bookings/{booking_id}/pay-online"
    headers = {"Authorization": f"Bearer {token}"}
    
    try:
        response = requests.post(url, headers=headers)
        return response.status_code == 200
    except:
        return False

def create_restaurant_order(token, customer_id):
    """Tạo đơn đặt món"""
    url = f"{BASE_URL}/api/services"
    headers = {"Authorization": f"Bearer {token}"}
    
    try:
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            return None
        
        services = response.json()
        if len(services) < 1:
            return None
        
        # Chọn 2 món đầu tiên
        items = []
        for service in services[:2]:
            items.append({
                "serviceId": service.get('serviceId'),
                "quantity": 2
            })
        
        # Tạo đơn
        order_url = f"{BASE_URL}/api/restaurant-orders"
        order_payload = {
            "customerId": customer_id,
            "items": items,
            "deliveryAddress": "Phòng 101",
            "paymentMethod": "QR"
        }
        
        response = requests.post(order_url, json=order_payload, headers=headers)
        if response.status_code == 201:
            return response.json()
        return None
    except Exception as e:
        print(f"❌ Lỗi tạo order: {e}")
        return None

def pay_restaurant_order(token, order_id):
    """Thanh toán đơn đặt món"""
    url = f"{BASE_URL}/api/restaurant-orders/{order_id}/pay"
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    payload = {
        "paymentMethod": "QR",
        "transactionId": f"TXN{datetime.now().strftime('%Y%m%d%H%M%S')}"
    }
    
    try:
        response = requests.post(url, json=payload, headers=headers)
        return response.status_code == 200
    except:
        return False

def check_dashboard_stats(admin_token):
    """Kiểm tra dashboard stats"""
    url = f"{BASE_URL}/api/reports/dashboard-stats"
    headers = {"Authorization": f"Bearer {admin_token}"}
    
    try:
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            return response.json()
        return None
    except:
        return None

def main():
    print("=" * 70)
    print("🧪 TEST DASHBOARD VÀ REPORTS - ĐẶT PHÒNG + ĐẶT MÓN ĂN")
    print("=" * 70)
    
    # Đăng nhập customer
    print("\n1️⃣ Đăng nhập khách hàng...")
    print("   ⚠️  LƯU Ý: Bạn cần có tài khoản khách hàng hợp lệ trong hệ thống")
    email = input("Email khách hàng: ").strip()
    if not email:
        print("❌ Email là bắt buộc!")
        sys.exit(1)
    password = input("Password: ").strip()
    if not password:
        print("❌ Password là bắt buộc!")
        sys.exit(1)
    
    token, customer_id = login(email, password)
    if not token:
        print("❌ Không thể đăng nhập khách hàng.")
        sys.exit(1)
    print(f"✅ Đăng nhập thành công! Customer ID: {customer_id}")
    
    # Tạo và thanh toán đặt phòng
    print("\n2️⃣ Tạo đặt phòng...")
    booking = create_booking(token, customer_id)
    if not booking:
        print("❌ Không thể tạo đặt phòng.")
        sys.exit(1)
    
    booking_id = booking.get('bookingId')
    booking_amount = booking.get('estimatedTotalAmount', 0)
    print(f"✅ Đặt phòng thành công! ID: {booking_id}, Số tiền: {booking_amount:,.0f} VNĐ")
    
    print("\n3️⃣ Thanh toán đặt phòng...")
    if pay_booking(token, booking_id):
        print(f"✅ Thanh toán đặt phòng thành công!")
    else:
        print("⚠️  Không thể thanh toán đặt phòng (có thể đã thanh toán hoặc lỗi)")
    
    time.sleep(1)
    
    # Tạo và thanh toán đơn đặt món
    print("\n4️⃣ Tạo đơn đặt món...")
    order = create_restaurant_order(token, customer_id)
    if not order:
        print("❌ Không thể tạo đơn đặt món.")
    else:
        order_id = order.get('orderId')
        order_amount = order.get('totalAmount', 0)
        print(f"✅ Đơn đặt món thành công! ID: {order_id}, Số tiền: {order_amount:,.0f} VNĐ")
        
        print("\n5️⃣ Thanh toán đơn đặt món...")
        if pay_restaurant_order(token, order_id):
            print(f"✅ Thanh toán đơn đặt món thành công!")
        else:
            print("⚠️  Không thể thanh toán đơn đặt món")
    
    # Đợi một chút để data được cập nhật
    print("\n⏳ Đợi 3 giây để data được cập nhật...")
    time.sleep(3)
    
    # Kiểm tra dashboard stats (cần admin token)
    print("\n6️⃣ Kiểm tra Dashboard Stats...")
    print("   (Cần đăng nhập admin để xem dashboard stats)")
    admin_email = input("Admin email (Enter để bỏ qua): ").strip()
    if admin_email:
        admin_password = input("Admin password: ").strip()
        admin_token, _ = login(admin_email, admin_password)
        if admin_token:
            stats = check_dashboard_stats(admin_token)
            if stats:
                print("\n📊 DASHBOARD STATS:")
                print(f"   Doanh thu hôm nay: {stats.get('todayRevenue', 0):,.0f} VNĐ")
                print(f"   Tỷ lệ lấp đầy: {stats.get('todayOccupancy', 0):.2f}%")
                print(f"   Đặt phòng đang hoạt động: {stats.get('activeBookings', 0)}")
                print(f"   Tăng trưởng tháng này: {stats.get('revenueGrowth', 0):.2f}%")
            else:
                print("⚠️  Không thể lấy dashboard stats")
    
    print("\n" + "=" * 70)
    print("✅ HOÀN TẤT TEST!")
    print("=" * 70)
    print("\n📝 Hãy kiểm tra:")
    print("   1. Trang Dashboard: /admin/html/index.html")
    print("   2. Trang Reports: /admin/html/reports.html")
    print("   3. Xem số liệu có được cập nhật không")
    print("=" * 70)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Đã hủy.")
        sys.exit(0)
    except Exception as e:
        print(f"\n❌ Lỗi: {e}")
        sys.exit(1)

