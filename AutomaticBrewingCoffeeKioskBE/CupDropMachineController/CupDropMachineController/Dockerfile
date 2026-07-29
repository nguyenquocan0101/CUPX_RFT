# Use the official Microsoft ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Thiết lập môi trường là Development
ENV ASPNETCORE_ENVIRONMENT=Development

# Sử dụng SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


# Copy toàn bộ source code vào container
COPY . .

# Restore dependencies
RUN dotnet restore "CupDropMachineController.csproj"

# Build và publish app ở chế độ Release, xuất ra thư mục /app/publish
RUN dotnet publish "CupDropMachineController.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy từ bước build sang runtime
COPY --from=build /app/publish .

# Copy file .env vào thư mục /app (để DotEnv tìm thấy)
COPY .env .

# Entry point để chạy console app
ENTRYPOINT ["dotnet", "CupDropMachineController.dll"]
