# Dockerfile ở root - Build context: Root của repo
# Sử dụng Dockerfile này nếu Render không tìm thấy QuanLyResort/Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies
COPY ["QuanLyResort/QuanLyResort.csproj", "QuanLyResort/"]
RUN dotnet restore "QuanLyResort/QuanLyResort.csproj"

# Copy toàn bộ source code
COPY QuanLyResort/ QuanLyResort/

# Build project
WORKDIR "/src/QuanLyResort"
RUN dotnet build "QuanLyResort.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/QuanLyResort"
RUN dotnet publish "QuanLyResort.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuanLyResort.dll"]

