# Multi-stage Dockerfile for MJAutomationCloud
# Supports both x64 and ARM64 architectures for Raspberry Pi deployment

# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy source code
COPY . .

# Build and publish the application
RUN dotnet restore -a $TARGETARCH
RUN dotnet build MJAutomationCloud/MJAutomationCloud.csproj -c Release -a $TARGETARCH --no-restore
RUN dotnet publish -c Release -a $TARGETARCH --no-restore --self-contained false -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install required packages for SQLite, HTTPS, and file operations
RUN apt-get update && apt-get install -y \
    sqlite3 \
    curl \
    openssl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Create application user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create directories for data persistence
RUN mkdir -p /app/data /app/files /app/logs /app/certs /app/letsencrypt \
    && chown -R appuser:appuser /app

# Copy published application
COPY --from=build /app/publish/ .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
# HTTP port for Let's Encrypt challenges and redirects
EXPOSE 8080
# HTTPS port for the main web application
EXPOSE 8443

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:8443;http://+:8080
ENV ASPNETCORE_HTTPS_PORT=8443
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/MJAutomationCloud.db"

# Health check (try HTTPS first, fallback to HTTP)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f -k https://localhost:8443/health || curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "MJAutomationCloud.dll"]
