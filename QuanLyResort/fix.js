const fs = require('fs');
const path = require('path');

// Đường dẫn tương đối từ file script đến file restaurant.html
const filePath = path.join(__dirname, 'wwwroot', 'customer', 'restaurant.html');

try {
    const content = fs.readFileSync(filePath, 'utf-8');
    const lines = content.split('\n');
    
    // Xóa từ dòng 766 đến 1876 (mảng index 0)
    lines.splice(766, 1111);
    
    fs.writeFileSync(filePath, lines.join('\n'), 'utf-8');
    console.log('✅ Đã dọn dẹp thành công đoạn HTML thừa trong file restaurant.html!');
} catch (error) {
    console.error('❌ Lỗi:', error.message);
}
