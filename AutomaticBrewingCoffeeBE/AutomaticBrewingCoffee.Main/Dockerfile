# Use the official Microsoft ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Production is the safe container default. Override explicitly for local development.
ENV ASPNETCORE_ENVIRONMENT=Production

# Use the official Microsoft .NET SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:8.0.404-alpine3.20-amd64 AS build
WORKDIR /src

COPY APIs ./APIs
COPY Domains ./Domains
COPY Repositories ./Repositories
COPY Services ./Services

# Restore only the production entry project. Its project references restore the
# required application layers without requiring the test project in this image.
RUN dotnet restore "APIs/APIs.csproj"

# Build the project
RUN dotnet build "APIs/APIs.csproj" -c Release --no-restore

# Publish the project
RUN dotnet publish "APIs/APIs.csproj" -c Release --no-restore -o /app/publish

# Use the runtime image for the final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "APIs.dll"]
