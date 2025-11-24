#!/usr/bin/env python3
"""
Script để sửa lỗi ẩn link social media trong footer
Loại bỏ class fadeInUp ftco-animated và thêm inline style để đảm bảo links luôn hiển thị
"""
import os
import re
from pathlib import Path

def fix_footer_links(file_path):
    """Sửa footer để đảm bảo social media links luôn hiển thị"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Pattern 1: Thay thế class fadeInUp ftco-animated và thêm inline style
        pattern1 = r'<li class="ftco-animate fadeInUp ftco-animated"><a href="#"><span class="icon-(\w+)"></span></a></li>'
        replacement1 = r'<li class="ftco-animate"><a href="#" style="display: block !important; visibility: visible !important; opacity: 1 !important;"><span class="icon-\1"></span></a></li>'
        content = re.sub(pattern1, replacement1, content)
        
        # Pattern 2: Nếu đã có style, chỉ cần loại bỏ fadeInUp ftco-animated
        pattern2 = r'<li class="ftco-animate fadeInUp ftco-animated">'
        replacement2 = r'<li class="ftco-animate">'
        content = re.sub(pattern2, replacement2, content)
        
        # Pattern 3: Thêm style vào thẻ <a> nếu chưa có
        pattern3 = r'<li class="ftco-animate"><a href="#"><span class="icon-(\w+)"></span></a></li>'
        replacement3 = r'<li class="ftco-animate"><a href="#" style="display: block !important; visibility: visible !important; opacity: 1 !important;"><span class="icon-\1"></span></a></li>'
        content = re.sub(pattern3, replacement3, content)
        
        # Nếu có thay đổi, ghi lại file
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"✅ Fixed: {file_path}")
            return True
        else:
            print(f"⚠️  No changes needed: {file_path}")
            return False
    except Exception as e:
        print(f"❌ Error fixing {file_path}: {e}")
        return False

def main():
    base_dir = Path("QuanLyResort/wwwroot/customer")
    html_files = [
        "index.html", "about.html", "contact.html", "rooms.html", 
        "rooms-single.html", "room-detail.html", "restaurant.html",
        "blog.html", "blog-single.html", "reviews.html", "faq.html",
        "support.html", "login.html", "register.html", "account.html",
        "my-bookings.html", "booking-details.html", "my-restaurant-orders.html",
        "my-tickets.html", "order-details.html", "order-success.html",
        "booking-success.html", "payment-momo.html"
    ]
    
    fixed = 0
    for html_file in html_files:
        file_path = base_dir / html_file
        if file_path.exists():
            if fix_footer_links(file_path):
                fixed += 1
        else:
            print(f"⚠️  File not found: {file_path}")
    
    print(f"\n✅ Fixed {fixed} files")

if __name__ == "__main__":
    main()

