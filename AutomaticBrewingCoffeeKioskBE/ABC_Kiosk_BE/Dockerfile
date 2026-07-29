# Use the official Microsoft ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Thiết lập môi trường là Development
ENV ASPNETCORE_ENVIRONMENT=Development

# Use the official Microsoft .NET SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
# COPY *.sln ./
# COPY DeviceConnectorTest ./DeviceConnectorTest
# COPY CupDroppingMachineTest ./CupDroppingMachineTest
# COPY IceMakerMachineTest ./IceMakerMachineTest
# COPY Kiosk.ApiService ./Kiosk.ApiService
# COPY Domain ./Domain
# COPY Repositories ./Repositories
# COPY Services ./Services

# Restore dependencies
RUN dotnet restore "ABC_Kiosk_BE.sln"

# Build the project
RUN dotnet build "Kiosk.ApiService/Kiosk.ApiService.csproj" -c Release

# Publish the project
RUN dotnet publish "Kiosk.ApiService/Kiosk.ApiService.csproj" -c Release --no-restore -o /app/publish

# Use the runtime image for the final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "Kiosk.ApiService.dll"]
