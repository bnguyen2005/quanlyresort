#!/bin/sh
# Entrypoint script để đọc PORT từ environment variable
# PORT phải là số nguyên từ 0 đến 65535

# Debug: Log tất cả environment variables liên quan đến PORT
echo "=== PORT Debug Info ==="
echo "PORT env var: '${PORT}'"
echo "PORT length: ${#PORT}"

# Lấy PORT từ environment variable và trim whitespace
PORT=$(echo "${PORT}" | tr -d '[:space:]')

# Nếu PORT rỗng hoặc không được set, dùng giá trị mặc định
if [ -z "$PORT" ] || [ "$PORT" = "" ]; then
    PORT=10000
    echo "PORT not set or empty, using default: $PORT"
fi

# Validate PORT là số nguyên hợp lệ (chỉ chứa số)
if ! echo "$PORT" | grep -qE '^[0-9]+$'; then
    echo "Error: PORT must be an integer. Got: '$PORT' (type: $(echo "$PORT" | od -c | head -1))"
    echo "Falling back to default PORT=10000"
    PORT=10000
fi

# Validate PORT trong range 0-65535
if [ "$PORT" -lt 0 ] || [ "$PORT" -gt 65535 ]; then
    echo "Error: PORT must be between 0 and 65535. Got: $PORT"
    echo "Falling back to default PORT=10000"
    PORT=10000
fi

# Set ASPNETCORE_URLS với PORT động
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
echo "Using PORT: $PORT"
echo "ASPNETCORE_URLS: $ASPNETCORE_URLS"
echo "======================="

# Chạy ứng dụng .NET
exec dotnet QuanLyResort.dll

