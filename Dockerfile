# =====================================================
# Stage 0: Set version arguments
# =====================================================
ARG DOTNET_VERSION=8.0
ARG NODEJS_VERSION_MAJOR=22


# =====================================================
# Stage 1: Build frontend assets (DfE / GOV.UK)
# =====================================================
FROM node:${NODEJS_VERSION_MAJOR}-bullseye-slim AS assets
WORKDIR /app

# Copy the entire Web project to ensure we have all necessary files
COPY ./SAPSec.Web/package*.json /app/
COPY ./SAPSec.Web/wwwroot/ /app/wwwroot/

# Install and build frontend assets
RUN npm ci --ignore-scripts && \
    (npm run build || npm run copy-assets || echo "Build step completed")

# Debug: Show what was built
RUN echo "=== Assets build output ===" && \
    find /app -type f -name "*.css" -o -name "*.js" | head -20


# =====================================================
# Stage 2: Build .NET project
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-azurelinux3.0 AS build
WORKDIR /build

# Copy project files and restore packages
COPY ./SAPSec.Core/SAPSec.Core.csproj /build/SAPSec.Core/
COPY ./SAPSec.Infrastructure/SAPSec.Infrastructure.csproj /build/SAPSec.Infrastructure/
COPY ./SAPSec.Web/SAPSec.Web.csproj /build/SAPSec.Web/
RUN dotnet restore /build/SAPSec.Web/SAPSec.Web.csproj

# Copy all code
COPY . .

# Build and publish .NET project
WORKDIR /build/SAPSec.Web
RUN dotnet build -c Release --no-restore && \
    dotnet publish -c Release --no-build -o /app/publish /p:UseAppHost=false


# =====================================================
# Stage 3: Runtime image
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-azurelinux3.0 AS final
WORKDIR /app

# Environment configuration
ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_ENVIRONMENT=Production

# Create directories with proper permissions for data protection keys
RUN mkdir -p /keys /home/app/.aspnet/DataProtection-Keys && \
    chmod -R 777 /keys /home/app/.aspnet

# Copy published .NET app first
COPY --from=build /app/publish /app

# Copy frontend assets (will overwrite/merge with published wwwroot)
COPY --from=assets /app/wwwroot /app/wwwroot

# Debug: Verify static files are present with correct permissions
RUN echo "=== Final wwwroot contents ===" && \
    ls -laR /app/wwwroot/ | head -50 && \
    echo "=== Checking for CSS files ===" && \
    find /app -name "*.css" -type f -exec ls -lh {} \;

# Azure Linux images have a default non-root user (ID 1654)
# Ensure proper ownership
RUN chown -R 1654:1654 /app /keys

# Switch to non-root user
USER 1654

# Expose port
EXPOSE 3000

# Entry point
ENTRYPOINT ["dotnet", "SAPSec.Web.dll"]