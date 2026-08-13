const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'QuanLyResort', 'wwwroot', 'customer', 'restaurant.html');

try {
    const content = fs.readFileSync(filePath, 'utf-8');
    const lines = content.split('\n');
    
    // Delete lines from 766 to 1876 (0-indexed)
    // Which corresponds to lines 767 to 1877 (1-indexed)
    lines.splice(766, 1111);
    
    fs.writeFileSync(filePath, lines.join('\n'), 'utf-8');
    console.log('✅ Đã dọn dẹp thành công đoạn HTML thừa trong file restaurant.html!');
} catch (error) {
    console.error('❌ Lỗi:', error.message);
}
