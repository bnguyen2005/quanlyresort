# Dockerfile ở root - Build context: Root của repo
# Sử dụng Dockerfile này nếu Render không tìm thấy QuanLyResort/Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# PORT sẽ được đọc từ environment variable khi runtime
# Sử dụng giá trị mặc định 10000 nếu không được set
ENV PORT=10000
EXPOSE 10000
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies (tối ưu cache)
COPY ["QuanLyResort/QuanLyResort.csproj", "QuanLyResort/"]
RUN dotnet restore "QuanLyResort/QuanLyResort.csproj" \
    --verbosity minimal

# Copy toàn bộ source code
COPY QuanLyResort/ QuanLyResort/

# Build và publish (không dùng --no-restore để tránh lỗi runtime)
WORKDIR "/src/QuanLyResort"
RUN dotnet publish "QuanLyResort.csproj" \
    -c Release \
    -o /app/publish \
    --verbosity minimal

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# Copy entrypoint script
COPY QuanLyResort/docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh
ENTRYPOINT ["/app/docker-entrypoint.sh"]

