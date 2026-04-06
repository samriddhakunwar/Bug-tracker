# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["BugTracker.API.csproj", "./"]

# Restore dependencies
RUN dotnet restore "BugTracker.API.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "BugTracker.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "BugTracker.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080 8443

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "BugTracker.API.dll"]
