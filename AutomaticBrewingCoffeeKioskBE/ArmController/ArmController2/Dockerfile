# Base image Windows Nano Server with .NET Framework 4.8.1
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8.1-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8080

# Use SDK image to build (nanoserver SDK may have limitations, test for your use case)
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1-windowsservercore-ltsc2022 AS build
WORKDIR /src

# Copy source code
COPY . .

# Restore and build the project
RUN msbuild ArmController.csproj /p:Configuration=Release /p:OutputPath=C:\app\publish

# Final image to run the app
FROM base AS final
WORKDIR /app

# Copy build output
COPY --from=build C:/app/publish .

# Copy .env if needed
COPY .env .

# Set the entry point to run your console application
ENTRYPOINT ["ArmController.exe"]
