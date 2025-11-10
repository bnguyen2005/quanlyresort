# Dockerfile ở root - Build context: Root của repo
# Sử dụng Dockerfile này nếu Render không tìm thấy QuanLyResort/Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies (tối ưu cache)
COPY ["QuanLyResort/QuanLyResort.csproj", "QuanLyResort/"]
RUN dotnet restore "QuanLyResort/QuanLyResort.csproj" \
    --verbosity minimal

# Copy toàn bộ source code
COPY QuanLyResort/ QuanLyResort/

# Build và publish trong một bước để tối ưu
WORKDIR "/src/QuanLyResort"
RUN dotnet publish "QuanLyResort.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --verbosity minimal \
    --runtime linux-x64 \
    --self-contained false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuanLyResort.dll"]

