import re
import os

file_path = r'd:\quanlyresort-main\quanlyresort-main\QuanLyResort\wwwroot\customer\rooms.html'
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
skip = False
for i, line in enumerate(lines):
    if '<!--' in line and 'overflow-x: hidden;' in lines[min(i+1, len(lines)-1)]:
        skip = True
    
    if skip and 'if (oldHtml) oldHtml.remove();' in line:
        skip = False
        new_lines.append(lines[i+1]) # add the </script> tag
        continue
        
    if not skip:
        new_lines.append(line)

with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
