#!/usr/bin/env python3
"""
Script để đặt phòng và thanh toán
Sử dụng: python create_booking_and_pay.py
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
    payload = {"email": email, "password": password}
    
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

def get_room_types(token):
    """Lấy danh sách loại phòng"""
    url = f"{BASE_URL}/api/room-types"
    headers = {"Authorization": f"Bearer {token}"}
    
    try:
        response = requests.get(url, headers=headers)
        if response.status_code == 200:
            room_types = response.json()
            print(f"✅ Lấy được {len(room_types)} loại phòng")
            return room_types
        return []
    except Exception as e:
        print(f"❌ Lỗi lấy loại phòng: {e}")
        return []

def create_booking(token, customer_id, room_type="Standard", days_ahead=7, nights=2):
    """Tạo đặt phòng"""
    url = f"{BASE_URL}/api/bookings"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    # Tính ngày check-in (7 ngày sau) và check-out
    check_in = datetime.now() + timedelta(days=days_ahead)
    check_out = check_in + timedelta(days=nights)
    
    payload = {
        "customerId": customer_id,
        "requestedRoomType": room_type,
        "checkInDate": check_in.isoformat(),
        "checkOutDate": check_out.isoformat(),
        "numberOfGuests": 2,
        "specialRequests": "Yêu cầu phòng view đẹp",
        "source": "Website"
    }
    
    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code == 201:
            booking = response.json()
            print(f"✅ Tạo đặt phòng thành công!")
            print(f"   Booking ID: {booking.get('bookingId')}")
            print(f"   Booking Code: {booking.get('bookingCode')}")
            print(f"   Tổng tiền: {booking.get('estimatedTotalAmount', 0):,.0f} VNĐ")
            print(f"   Check-in: {check_in.strftime('%d/%m/%Y')}")
            print(f"   Check-out: {check_out.strftime('%d/%m/%Y')}")
            return booking
        else:
            print(f"❌ Lỗi tạo đặt phòng: {response.status_code} - {response.text}")
            return None
    except Exception as e:
        print(f"❌ Lỗi khi tạo đặt phòng: {e}")
        return None

def pay_booking(token, booking_id):
    """Thanh toán đặt phòng"""
    url = f"{BASE_URL}/api/bookings/{booking_id}/pay-online"
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    try:
        response = requests.post(url, headers=headers)
        if response.status_code == 200:
            result = response.json()
            print(f"✅ Thanh toán thành công!")
            print(f"   Status: {result.get('status')}")
            print(f"   Invoice Number: {result.get('invoiceNumber', 'N/A')}")
            return result
        else:
            print(f"❌ Lỗi thanh toán: {response.status_code} - {response.text}")
            return None
    except Exception as e:
        print(f"❌ Lỗi khi thanh toán: {e}")
        return None

def main():
    print("=" * 60)
    print("🏨 SCRIPT ĐẶT PHÒNG VÀ THANH TOÁN")
    print("=" * 60)
    
    # Thông tin đăng nhập
    email = input("Nhập email khách hàng (hoặc Enter để dùng mặc định): ").strip()
    if not email:
        email = "customer@example.com"
    
    password = input("Nhập mật khẩu (hoặc Enter để dùng mặc định): ").strip()
    if not password:
        password = "Customer123!"
    
    # Đăng nhập
    print("\n📝 Đang đăng nhập...")
    token, customer_id = login(email, password)
    if not token:
        print("❌ Không thể đăng nhập.")
        sys.exit(1)
    
    # Lấy danh sách loại phòng
    print("\n🏠 Đang lấy danh sách loại phòng...")
    room_types = get_room_types(token)
    if room_types:
        print("\n📋 Danh sách loại phòng:")
        for i, rt in enumerate(room_types[:5], 1):
            print(f"   {i}. {rt.get('typeName', 'N/A')}")
        room_type = room_types[0].get('typeName', 'Standard') if room_types else 'Standard'
    else:
        room_type = 'Standard'
    
    # Tạo đặt phòng
    print(f"\n🛒 Đang tạo đặt phòng (loại phòng: {room_type})...")
    booking = create_booking(token, customer_id, room_type=room_type, days_ahead=7, nights=2)
    
    if not booking:
        print("❌ Không thể tạo đặt phòng.")
        sys.exit(1)
    
    booking_id = booking.get('bookingId')
    total_amount = booking.get('estimatedTotalAmount', 0)
    
    # Thanh toán
    print(f"\n💳 Đang thanh toán đặt phòng {booking_id}...")
    print(f"   Tổng tiền: {total_amount:,.0f} VNĐ")
    
    payment_result = pay_booking(token, booking_id)
    
    if payment_result:
        print("\n" + "=" * 60)
        print("✅ HOÀN TẤT!")
        print("=" * 60)
        print(f"Booking ID: {booking_id}")
        print(f"Booking Code: {booking.get('bookingCode')}")
        print(f"Tổng tiền: {total_amount:,.0f} VNĐ")
        print(f"Trạng thái: {payment_result.get('status', 'Paid')}")
        print("=" * 60)
    else:
        print("\n❌ Không thể thanh toán đặt phòng.")
        print(f"   Đặt phòng đã được tạo với ID: {booking_id}")

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Đã hủy bởi người dùng.")
        sys.exit(0)
    except Exception as e:
        print(f"\n❌ Lỗi không mong đợi: {e}")
        sys.exit(1)

