# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY *.csproj .
RUN dotnet restore

# Copy everything else
COPY . .

# Build
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Entry point
ENTRYPOINT ["dotnet", "ParkingSystem.dll"]