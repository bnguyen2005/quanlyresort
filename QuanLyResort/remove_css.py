import re

path = 'd:/quanlyresort-main/quanlyresort-main/QuanLyResort/wwwroot/customer/rooms.html'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace everything from <style> to </style> with the link
new_content = re.sub(r'<style>.*?</style>', '<link rel="stylesheet" href="css/taste-rooms.css">', content, flags=re.DOTALL)

with open(path, 'w', encoding='utf-8') as f:
    f.write(new_content)

print("Replaced CSS")
